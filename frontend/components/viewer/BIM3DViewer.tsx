'use client';

import React, { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import { BIMElement } from '../../types';
import { Eye, Layers, Maximize2, RotateCcw, Box, Info, X, Video, StopCircle } from 'lucide-react';

interface BIM3DViewerProps {
  elements: BIMElement[];
  builtupArea: number;
  floorsCount: number;
}

export default function BIM3DViewer({ elements, builtupArea, floorsCount }: BIM3DViewerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [selectedElement, setSelectedElement] = useState<BIMElement | null>(null);
  const [showCeilings, setShowCeilings] = useState(false);
  const [showRoof, setShowRoof] = useState(true);
  const [viewMode, setViewMode] = useState<'orbit' | 'top' | 'interior'>('orbit');
  const [isRecording, setIsRecording] = useState(false);
  const [recordSeconds, setRecordSeconds] = useState(0);

  const sceneRef = useRef<THREE.Scene | null>(null);
  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const meshMapRef = useRef<Map<string, { mesh: THREE.Mesh; element: BIMElement }>>(new Map());
  const selectedMeshRef = useRef<THREE.Mesh | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    // 1. Scene setup
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0f172a); // Slate-900
    sceneRef.current = scene;

    // 2. Camera setup
    const width = containerRef.current.clientWidth || 800;
    const height = containerRef.current.clientHeight || 550;
    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    camera.position.set(16, 14, 20);
    camera.lookAt(6, 2, 6);
    cameraRef.current = camera;

    // 3. Renderer setup
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true, powerPreference: 'high-performance' });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;

    containerRef.current.innerHTML = '';
    containerRef.current.appendChild(renderer.domElement);
    rendererRef.current = renderer;

    // 4. Lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
    scene.add(ambientLight);

    const dirLight = new THREE.DirectionalLight(0xffffff, 0.9);
    dirLight.position.set(20, 35, 20);
    dirLight.castShadow = true;
    dirLight.shadow.mapSize.width = 2048;
    dirLight.shadow.mapSize.height = 2048;
    scene.add(dirLight);

    const fillLight = new THREE.DirectionalLight(0xffffff, 0.4);
    fillLight.position.set(-15, 10, -15);
    scene.add(fillLight);

    // 5. Ground Grid
    const grid = new THREE.GridHelper(50, 50, 0xdc2626, 0x334155);
    grid.position.y = -0.01;
    scene.add(grid);

    // 6. Build 3D Meshes from BIM Elements
    meshMapRef.current.clear();
    const materialCache: Record<string, THREE.Material> = {
      Wall: new THREE.MeshStandardMaterial({ color: 0xd1d5db, roughness: 0.6, metalness: 0.1 }),
      Floor: new THREE.MeshStandardMaterial({ color: 0x94a3b8, roughness: 0.4, metalness: 0.2 }),
      Ceiling: new THREE.MeshStandardMaterial({ color: 0xf1f5f9, roughness: 0.9 }),
      Roof: new THREE.MeshStandardMaterial({ color: 0x475569, roughness: 0.7 }),
      Door: new THREE.MeshStandardMaterial({ color: 0xb45309, roughness: 0.5 }), // Warm timber
      Window: new THREE.MeshPhysicalMaterial({ color: 0x38bdf8, transparent: true, opacity: 0.45, roughness: 0.1, transmission: 0.6 }),
      Default: new THREE.MeshStandardMaterial({ color: 0x64748b })
    };

    elements.forEach((elem) => {
      let geo: THREE.BufferGeometry;
      const w = elem.width || elem.thickness || 0.23;
      const h = elem.height || 3.0;
      const l = elem.length || 4.0;

      if (elem.type === 'Wall') {
        geo = new THREE.BoxGeometry(w, h, l);
      } else if (elem.type === 'Floor' || elem.type === 'Ceiling' || elem.type === 'Roof') {
        geo = new THREE.BoxGeometry(elem.width || 4.0, elem.height || 0.15, elem.length || 4.0);
      } else if (elem.type === 'Door') {
        geo = new THREE.BoxGeometry(0.1, h, l);
      } else if (elem.type === 'Window') {
        geo = new THREE.BoxGeometry(0.08, h, l);
      } else {
        geo = new THREE.BoxGeometry(w, h, l);
      }

      const mat = (materialCache[elem.type] || materialCache.Default).clone();
      const mesh = new THREE.Mesh(geo, mat);
      mesh.position.set(elem.position[0], elem.position[1], elem.position[2]);
      mesh.castShadow = true;
      mesh.receiveShadow = true;
      mesh.userData = { bimElement: elem };

      // Hide ceilings by default for interior viewing
      if (elem.type === 'Ceiling') {
        mesh.visible = false;
      }

      scene.add(mesh);
      meshMapRef.current.set(elem.id, { mesh, element: elem });
    });

    // 7. Interactive Orbiting (Pan / Rotate / Zoom)
    let isDragging = false;
    let prevMousePos = { x: 0, y: 0 };
    let spherical = new THREE.Spherical(26, Math.PI / 3.5, Math.PI / 4);
    let target = new THREE.Vector3(6, 2, 6);

    const updateCamera = () => {
      spherical.radius = Math.max(4, Math.min(60, spherical.radius));
      spherical.phi = Math.max(0.1, Math.min(Math.PI / 2.05, spherical.phi));
      camera.position.setFromSpherical(spherical).add(target);
      camera.lookAt(target);
    };
    updateCamera();

    const dom = renderer.domElement;

    const onMouseDown = (e: MouseEvent) => {
      isDragging = true;
      prevMousePos = { x: e.clientX, y: e.clientY };
    };

    const onMouseMove = (e: MouseEvent) => {
      if (!isDragging) return;
      const dx = e.clientX - prevMousePos.x;
      const dy = e.clientY - prevMousePos.y;

      if (e.buttons === 1) {
        // Left click: Rotate
        spherical.theta -= dx * 0.008;
        spherical.phi -= dy * 0.008;
      } else if (e.buttons === 2) {
        // Right click: Pan
        const panSpeed = 0.03;
        target.x -= dx * panSpeed;
        target.z -= dy * panSpeed;
      }
      prevMousePos = { x: e.clientX, y: e.clientY };
      updateCamera();
    };

    const onMouseUp = () => { isDragging = false; };

    const onWheel = (e: WheelEvent) => {
      e.preventDefault();
      spherical.radius += e.deltaY * 0.02;
      updateCamera();
    };

    // Raycast on Click to select BIM element
    const raycaster = new THREE.Raycaster();
    const mouse = new THREE.Vector2();

    const onClick = (e: MouseEvent) => {
      const rect = dom.getBoundingClientRect();
      mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
      mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

      raycaster.setFromCamera(mouse, camera);
      const meshes = Array.from(meshMapRef.current.values()).map(v => v.mesh).filter(m => m.visible);
      const intersects = raycaster.intersectObjects(meshes, false);

      if (intersects.length > 0) {
        const hit = intersects[0].object as THREE.Mesh;
        const elemData = hit.userData.bimElement as BIMElement;

        // Reset previous selection
        if (selectedMeshRef.current && (selectedMeshRef.current.material as THREE.MeshStandardMaterial)) {
          (selectedMeshRef.current.material as THREE.MeshStandardMaterial).emissive.setHex(0x000000);
        }

        // Highlight new selection with crimson red glow
        selectedMeshRef.current = hit;
        (hit.material as THREE.MeshStandardMaterial).emissive.setHex(0xdc2626);
        setSelectedElement(elemData);
      }
    };

    dom.addEventListener('mousedown', onMouseDown);
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    dom.addEventListener('wheel', onWheel, { passive: false });
    dom.addEventListener('click', onClick);
    dom.addEventListener('contextmenu', (e) => e.preventDefault());

    // 8. Render Loop
    let animId: number;
    const animate = () => {
      animId = requestAnimationFrame(animate);
      renderer.render(scene, camera);
    };
    animate();

    // 9. Resize observer
    const resizeObserver = new ResizeObserver(() => {
      if (!containerRef.current) return;
      const w = containerRef.current.clientWidth;
      const h = containerRef.current.clientHeight;
      camera.aspect = w / h;
      camera.updateProjectionMatrix();
      renderer.setSize(w, h);
    });
    resizeObserver.observe(containerRef.current);

    return () => {
      cancelAnimationFrame(animId);
      dom.removeEventListener('mousedown', onMouseDown);
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      dom.removeEventListener('wheel', onWheel);
      dom.removeEventListener('click', onClick);
      resizeObserver.disconnect();
      renderer.dispose();
    };
  }, [elements]);

  // Handle visibility toggles
  const handleToggleCeilings = () => {
    const next = !showCeilings;
    setShowCeilings(next);
    meshMapRef.current.forEach(({ mesh, element }) => {
      if (element.type === 'Ceiling') mesh.visible = next;
    });
  };

  const handleToggleRoof = () => {
    const next = !showRoof;
    setShowRoof(next);
    meshMapRef.current.forEach(({ mesh, element }) => {
      if (element.type === 'Roof') mesh.visible = next;
    });
  };

  const handleResetCamera = () => {
    if (!cameraRef.current) return;
    cameraRef.current.position.set(16, 14, 20);
    cameraRef.current.lookAt(6, 2, 6);
  };

  const startRecording = () => {
    if (!rendererRef.current) return;
    const canvas = rendererRef.current.domElement;
    try {
      const stream = canvas.captureStream ? canvas.captureStream(30) : null;
      if (!stream) {
        alert('Canvas video streaming is not supported on this browser.');
        return;
      }
      const chunks: Blob[] = [];
      const recorder = new MediaRecorder(stream, { mimeType: 'video/webm' });
      recorder.ondataavailable = (e) => {
        if (e.data && e.data.size > 0) chunks.push(e.data);
      };
      recorder.onstop = () => {
        const blob = new Blob(chunks, { type: 'video/webm' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `smartbuild-walkthrough-${Date.now()}.webm`;
        a.click();
        URL.revokeObjectURL(url);
      };
      recorder.start();
      mediaRecorderRef.current = recorder;
      setIsRecording(true);
      setRecordSeconds(0);
      timerRef.current = setInterval(() => {
        setRecordSeconds((s) => s + 1);
      }, 1000);
    } catch (err) {
      console.error('Recording error:', err);
      alert('Walkthrough video recording could not be started.');
    }
  };

  const stopRecording = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== 'inactive') {
      mediaRecorderRef.current.stop();
    }
    setIsRecording(false);
  };

  const formatSeconds = (sec: number) => {
    const mins = Math.floor(sec / 60);
    const secs = sec % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="relative w-full h-[620px] rounded-xl overflow-hidden border border-slate-700 bg-slate-950 shadow-2xl flex flex-col">
      {/* 3D Viewport canvas container */}
      <div ref={containerRef} className="w-full h-full cursor-grab active:cursor-grabbing" />

      {/* Top Floating Controls Bar */}
      <div className="absolute top-4 left-4 right-4 flex items-center justify-between pointer-events-none">
        <div className="bg-slate-900/90 backdrop-blur-md px-4 py-2 rounded-lg border border-slate-700 text-xs text-slate-300 pointer-events-auto flex items-center gap-3 shadow-lg">
          <span className="font-semibold text-red-500">BIM 3D Model</span>
          <span className="text-slate-500">|</span>
          <span>{elements.length} Components</span>
          <span className="text-slate-500">|</span>
          <span>{builtupArea} m² Built-Up</span>
          <span className="text-slate-500">|</span>
          <span>{floorsCount} {floorsCount === 1 ? 'Floor' : 'Floors'}</span>
        </div>

        <div className="flex items-center gap-2 pointer-events-auto">
          <button
            onClick={handleToggleCeilings}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors flex items-center gap-1.5 shadow ${
              showCeilings 
                ? 'bg-red-600 border-red-500 text-white shadow-red-600/30' 
                : 'bg-slate-800/90 border-slate-700 text-slate-300 hover:bg-slate-700'
            }`}
          >
            <Layers className="w-3.5 h-3.5" />
            {showCeilings ? 'Hide Ceilings' : 'Show Ceilings'}
          </button>

          <button
            onClick={handleToggleRoof}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors flex items-center gap-1.5 shadow ${
              showRoof 
                ? 'bg-red-600 border-red-500 text-white shadow-red-600/30' 
                : 'bg-slate-800/90 border-slate-700 text-slate-300 hover:bg-slate-700'
            }`}
          >
            <Box className="w-3.5 h-3.5" />
            {showRoof ? 'Roof On' : 'Roof Off'}
          </button>

          {isRecording ? (
            <button
              onClick={stopRecording}
              className="px-3 py-1.5 rounded-lg text-xs font-semibold bg-rose-600 hover:bg-rose-500 text-white flex items-center gap-1.5 shadow-lg animate-pulse"
              title="Stop Recording and Download Video"
            >
              <StopCircle className="w-3.5 h-3.5" />
              Stop REC ({formatSeconds(recordSeconds)})
            </button>
          ) : (
            <button
              onClick={startRecording}
              className="px-3 py-1.5 rounded-lg text-xs font-medium bg-slate-800/90 border border-slate-700 text-slate-300 hover:text-white hover:bg-slate-700 transition flex items-center gap-1.5 shadow"
              title="Record Walkthrough Video (WebM)"
            >
              <Video className="w-3.5 h-3.5 text-rose-400" />
              Record Video
            </button>
          )}

          <button
            onClick={handleResetCamera}
            className="p-2 rounded-lg bg-slate-800/90 border border-slate-700 text-slate-300 hover:bg-slate-700 transition shadow"
            title="Reset Camera"
          >
            <RotateCcw className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      {/* Selected Element BIM Metadata Inspector Card */}
      {selectedElement && (
        <div className="absolute bottom-4 left-4 max-w-sm w-full bg-slate-900/95 backdrop-blur-md border border-red-500/50 rounded-xl p-4 text-xs text-slate-300 shadow-2xl animate-fade-in z-20">
          <div className="flex items-center justify-between pb-2 mb-3 border-b border-slate-700">
            <div className="flex items-center gap-2">
              <span className="w-2.5 h-2.5 rounded-full bg-red-500 animate-pulse" />
              <span className="font-bold text-sm text-white">{selectedElement.type} Element</span>
            </div>
            <button
              onClick={() => setSelectedElement(null)}
              className="text-slate-400 hover:text-white"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          <div className="grid grid-cols-2 gap-2 text-slate-400">
            <div>
              <span className="text-slate-500 block">Element ID:</span>
              <span className="font-mono text-slate-200">{selectedElement.id.slice(0, 14)}</span>
            </div>
            <div>
              <span className="text-slate-500 block">Room / Zone:</span>
              <span className="text-slate-200 font-medium">{selectedElement.room_name || 'Envelope'}</span>
            </div>
            <div>
              <span className="text-slate-500 block">Material:</span>
              <span className="text-slate-200 font-medium">{selectedElement.material}</span>
            </div>
            <div>
              <span className="text-slate-500 block">Floor Level:</span>
              <span className="text-slate-200">Level {selectedElement.floor_level}</span>
            </div>
            <div>
              <span className="text-slate-500 block">Surface Area:</span>
              <span className="text-red-400 font-semibold">{selectedElement.area.toFixed(2)} m²</span>
            </div>
            <div>
              <span className="text-slate-500 block">Volume:</span>
              <span className="text-red-400 font-semibold">{selectedElement.volume.toFixed(3)} m³</span>
            </div>
          </div>

          <div className="mt-3 pt-2 border-t border-slate-800 text-[11px] text-slate-500 flex items-center justify-between">
            <span>Dimensions:</span>
            <span className="font-mono text-slate-300">
              {(selectedElement.length || 0).toFixed(1)}m × {(selectedElement.height || 0).toFixed(1)}m × {(selectedElement.thickness || selectedElement.width || 0).toFixed(2)}m
            </span>
          </div>
        </div>
      )}

      {/* Bottom Hint */}
      <div className="absolute bottom-4 right-4 bg-slate-900/80 backdrop-blur-sm px-3 py-1.5 rounded-lg border border-slate-800 text-[11px] text-slate-400 pointer-events-none">
        Click any element to inspect BIM metadata • Left drag rotate • Right drag pan • Scroll zoom
      </div>
    </div>
  );
}
