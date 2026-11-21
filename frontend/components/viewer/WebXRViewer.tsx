'use client';

import React, { useEffect, useState, useRef } from 'react';
import * as THREE from 'three';
import { BIMElement } from '../../types';
import { Compass, Glasses, Smartphone, Eye, CheckCircle2, AlertTriangle, ArrowLeft } from 'lucide-react';

interface WebXRViewerProps {
  mode: 'vr' | 'ar';
  elements: BIMElement[];
  onBack: () => void;
  projectId?: string;
}

export default function WebXRViewer({ mode, elements, onBack, projectId = 'default' }: WebXRViewerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [xrSupported, setXrSupported] = useState<boolean | null>(null);
  const [isSessionActive, setIsSessionActive] = useState(false);
  const [arScale, setArScale] = useState<number>(0.05); // 1:20 scale miniature default
  const [rotationY, setRotationY] = useState<number>(0);
  const [activeRoomName, setActiveRoomName] = useState<string>('Entrance');

  const moveStateRef = useRef({ forward: 0, right: 0 });
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const teleportRef = useRef<((pos: [number, number, number], name: string) => void) | null>(null);

  // Extract unique rooms for instant teleportation
  const roomsList = React.useMemo(() => {
    const list: { name: string; position: [number, number, number] }[] = [];
    const seen = new Set<string>();
    elements.forEach(e => {
      if ((e.type === 'Floor' || e.type === 'Roof') && e.room_name && !seen.has(e.room_name)) {
        seen.add(e.room_name);
        list.push({ name: e.room_name, position: e.position });
      }
    });
    if (list.length === 0) {
      list.push({ name: 'Main Entrance', position: [2, 0, 2] });
    }
    return list;
  }, [elements]);

  // Check WebXR device capabilities on mount
  useEffect(() => {
    async function checkCapabilities() {
      if (typeof window !== 'undefined' && 'xr' in navigator && (navigator as any).xr) {
        try {
          const sessionType = mode === 'vr' ? 'immersive-vr' : 'immersive-ar';
          const isSupported = await (navigator as any).xr.isSessionSupported(sessionType);
          setXrSupported(isSupported);
        } catch {
          setXrSupported(false);
        }
      } else {
        setXrSupported(false);
      }
    }
    checkCapabilities();
  }, [mode]);

  // Three.js Render Scene with Interactive Walkthrough / AR View
  useEffect(() => {
    if (!containerRef.current) return;

    const width = containerRef.current.clientWidth || 800;
    const height = containerRef.current.clientHeight || 550;

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(mode === 'ar' ? 0x050814 : 0x0b1120);

    const camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 1000);
    // 1.7m human eye-level for VR/Walkthrough mode
    camera.position.set(2, 1.7, 2);

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

    containerRef.current.innerHTML = '';
    containerRef.current.appendChild(renderer.domElement);

    // Lighting
    const ambient = new THREE.AmbientLight(0xffffff, 0.8);
    scene.add(ambient);
    const dir = new THREE.DirectionalLight(0xffffff, 0.7);
    dir.position.set(10, 20, 10);
    scene.add(dir);

    // Floor grid indicator
    const grid = new THREE.GridHelper(30, 30, mode === 'ar' ? 0xef4444 : 0xdc2626, 0x1e293b);
    scene.add(grid);

    // Root Group for transformation (Scaling & Rotating in AR)
    const houseGroup = new THREE.Group();
    scene.add(houseGroup);

    // Load active styling customizations
    let customSettings: any = null;
    try {
      const saved = localStorage.getItem(`smartbuild_customization_${projectId}`);
      if (saved) customSettings = JSON.parse(saved);
    } catch {}

    const defaultWallColor = customSettings?.wallColor ? new THREE.Color(customSettings.wallColor).getHex() : 0xd1d5db;
    const defaultDoorColor = customSettings?.doorColor ? new THREE.Color(customSettings.doorColor).getHex() : 0xb45309;
    const grillColorHex = customSettings?.grillColor ? new THREE.Color(customSettings.grillColor).getHex() : 0x18181b;
    const showGrills = customSettings?.showGrills !== false;

    // Build building elements
    elements.forEach((elem) => {
      const w = elem.width || elem.thickness || 0.23;
      const h = elem.height || 3.0;
      const l = elem.length || 4.0;
      const geo = new THREE.BoxGeometry(w, h, l);

      let color = defaultWallColor;
      if (elem.type === 'Wall') {
        color = customSettings?.customWallColors?.[elem.id] 
          ? new THREE.Color(customSettings.customWallColors[elem.id]).getHex() 
          : defaultWallColor;
      } else if (elem.type === 'Door') {
        color = customSettings?.customDoorColors?.[elem.id]
          ? new THREE.Color(customSettings.customDoorColors[elem.id]).getHex()
          : defaultDoorColor;
      } else if (elem.type === 'Floor') {
        const matType = (elem.material || '').toLowerCase();
        if (matType.includes('hardwood') || matType.includes('wood')) color = 0xb45309;
        else if (matType.includes('marble')) color = 0xf8fafc;
        else if (matType.includes('ceramic')) color = 0xe2e8f0;
        else if (matType.includes('granite')) color = 0x334155;
        else color = 0xcbd5e1;
      } else if (elem.type === 'Window') color = 0x38bdf8;
      else if (elem.type === 'Roof') color = 0x475569;

      const mat = new THREE.MeshStandardMaterial({
        color,
        transparent: elem.type === 'Window',
        opacity: elem.type === 'Window' ? 0.45 : 1.0,
        roughness: elem.type === 'Wall' ? 0.5 : 0.4
      });

      const mesh = new THREE.Mesh(geo, mat);
      mesh.position.set(elem.position[0], elem.position[1], elem.position[2]);
      houseGroup.add(mesh);

      // Add window safety grills if enabled
      if (elem.type === 'Window' && showGrills) {
        const grillMat = new THREE.MeshStandardMaterial({ color: grillColorHex, metalness: 0.85, roughness: 0.3 });
        const numBars = Math.max(3, Math.floor(l / 0.2));
        const step = (l - 0.1) / (numBars + 1);
        for (let i = 1; i <= numBars; i++) {
          const vBar = new THREE.Mesh(new THREE.CylinderGeometry(0.008, 0.008, h - 0.05, 8), grillMat);
          vBar.position.set(elem.position[0], elem.position[1], elem.position[2] - l / 2 + 0.05 + i * step);
          houseGroup.add(vBar);
        }
      }
    });

    // Add placed pots if any
    if (customSettings?.pots && Array.isArray(customSettings.pots)) {
      customSettings.pots.forEach((pot: any) => {
        const potGroup = new THREE.Group();
        potGroup.position.set(pot.position[0], pot.position[1], pot.position[2]);
        potGroup.scale.setScalar(pot.scale || 1.0);

        const potColorNum = new THREE.Color(pot.potColor || '#c25e36').getHex();
        const potMat = new THREE.MeshStandardMaterial({ color: potColorNum, roughness: 0.6 });
        const potMesh = new THREE.Mesh(new THREE.CylinderGeometry(0.24, 0.16, 0.48, 16), potMat);
        potMesh.position.y = 0.24;
        potGroup.add(potMesh);

        const leafMat = new THREE.MeshStandardMaterial({ color: 0x15803d, roughness: 0.6 });
        const foliage = new THREE.Mesh(new THREE.SphereGeometry(0.25, 12, 10), leafMat);
        foliage.position.y = 0.48 + 0.2;
        foliage.scale.set(1.1, 0.9, 1.1);
        potGroup.add(foliage);

        houseGroup.add(potGroup);
      });
    }

    cameraRef.current = camera;
    teleportRef.current = (pos: [number, number, number], name: string) => {
      camera.position.set(pos[0], 1.7, pos[2]);
      setActiveRoomName(name);
    };

    // Walkthrough controls (WASD + Mouse Drag + On-Screen Touch D-Pad)
    let isMouseDown = false;
    let yaw = 0;
    let pitch = 0;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'w' || e.key === 'ArrowUp') moveStateRef.current.forward = 1;
      if (e.key === 's' || e.key === 'ArrowDown') moveStateRef.current.forward = -1;
      if (e.key === 'a' || e.key === 'ArrowLeft') moveStateRef.current.right = -1;
      if (e.key === 'd' || e.key === 'ArrowRight') moveStateRef.current.right = 1;
    };

    const onKeyUp = (e: KeyboardEvent) => {
      if (['w', 's', 'ArrowUp', 'ArrowDown'].includes(e.key)) moveStateRef.current.forward = 0;
      if (['a', 'd', 'ArrowLeft', 'ArrowRight'].includes(e.key)) moveStateRef.current.right = 0;
    };

    const dom = renderer.domElement;
    let prevMouse = { x: 0, y: 0 };

    const onMouseDown = (e: MouseEvent) => {
      isMouseDown = true;
      prevMouse = { x: e.clientX, y: e.clientY };
    };

    const onMouseMove = (e: MouseEvent) => {
      if (!isMouseDown) return;
      const dx = e.clientX - prevMouse.x;
      const dy = e.clientY - prevMouse.y;
      yaw -= dx * 0.003;
      pitch = Math.max(-Math.PI / 3, Math.min(Math.PI / 3, pitch - dy * 0.003));
      prevMouse = { x: e.clientX, y: e.clientY };
    };

    const onMouseUp = () => { isMouseDown = false; };

    window.addEventListener('keydown', onKeyDown);
    window.addEventListener('keyup', onKeyUp);
    dom.addEventListener('mousedown', onMouseDown);
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);

    // Animation Loop
    let animId: number;
    const animate = () => {
      animId = requestAnimationFrame(animate);

      // Apply AR scale & rotation
      if (mode === 'ar') {
        houseGroup.scale.set(arScale, arScale, arScale);
        houseGroup.rotation.y = rotationY;
      }

      // First-person movement
      if (moveStateRef.current.forward !== 0 || moveStateRef.current.right !== 0) {
        const speed = 0.08;
        const forwardVector = new THREE.Vector3(Math.sin(yaw), 0, Math.cos(yaw)).normalize();
        const rightVector = new THREE.Vector3(Math.cos(yaw), 0, -Math.sin(yaw)).normalize();

        camera.position.addScaledVector(forwardVector, -moveStateRef.current.forward * speed);
        camera.position.addScaledVector(rightVector, moveStateRef.current.right * speed);
        camera.position.y = 1.7; // maintain 1.7m eye-level
      }

      camera.rotation.set(0, 0, 0);
      camera.rotation.order = 'YXZ';
      camera.rotation.y = yaw;
      camera.rotation.x = pitch;

      renderer.render(scene, camera);
    };
    animate();

    return () => {
      cancelAnimationFrame(animId);
      window.removeEventListener('keydown', onKeyDown);
      window.removeEventListener('keyup', onKeyUp);
      dom.removeEventListener('mousedown', onMouseDown);
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      renderer.dispose();
    };
  }, [elements, mode, arScale, rotationY]);

  return (
    <div className="relative w-full h-[640px] rounded-2xl overflow-hidden border border-slate-200 bg-slate-950 flex flex-col shadow-xl select-none">
      {/* Viewport canvas */}
      <div ref={containerRef} className="w-full h-full cursor-crosshair" />

      {/* Top Banner with Navigation and Device Capability */}
      <div className="absolute top-4 left-4 right-4 flex items-center justify-between pointer-events-none z-20">
        <button
          onClick={onBack}
          className="pointer-events-auto bg-white/95 hover:bg-slate-50 text-slate-800 px-3.5 py-2 rounded-xl border border-slate-200 text-xs font-semibold flex items-center gap-1.5 shadow-md transition"
        >
          <ArrowLeft className="w-4 h-4 text-red-600" />
          Back to 3D BIM Viewer
        </button>

        <div className="pointer-events-auto bg-white/95 backdrop-blur-md px-4 py-2 rounded-xl border border-slate-200 text-xs text-slate-800 flex items-center gap-2 shadow-md">
          {xrSupported === true ? (
            <span className="flex items-center gap-1.5 text-emerald-600 font-semibold">
              <CheckCircle2 className="w-4 h-4 text-emerald-600" />
              WebXR {mode.toUpperCase()} Hardware Active
            </span>
          ) : (
            <span className="flex items-center gap-1.5 text-amber-600 font-semibold">
              <AlertTriangle className="w-4 h-4 text-amber-500" />
              WebXR Simulation Engine • 60 FPS Hardware Fallback
            </span>
          )}
        </div>
      </div>

      {/* Room Teleport Quick Jump Selector (VR Mode) */}
      {mode === 'vr' && (
        <div className="absolute top-18 left-4 pointer-events-auto bg-white/95 backdrop-blur-md border border-slate-200 rounded-2xl p-3 shadow-xl max-w-xs z-20">
          <div className="flex items-center justify-between mb-2">
            <span className="text-[10px] font-bold text-slate-500 uppercase tracking-wider">
              Room Teleport
            </span>
            <span className="text-[10px] font-semibold text-red-600 bg-red-50 px-2 py-0.5 rounded-full border border-red-100">
              {activeRoomName}
            </span>
          </div>
          <div className="flex flex-wrap gap-1.5 max-h-36 overflow-y-auto">
            {roomsList.map(r => (
              <button
                key={r.name}
                onClick={() => {
                  if (teleportRef.current) {
                    teleportRef.current([r.position[0], 1.7, r.position[2]], r.name);
                  }
                }}
                className={`px-2.5 py-1 rounded-lg text-[11px] font-medium transition ${
                  activeRoomName === r.name
                    ? 'bg-red-600 text-white shadow-xs'
                    : 'bg-slate-100 hover:bg-slate-200 text-slate-700'
                }`}
              >
                {r.name}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* VR Directional Touch / Click D-Pad Controls */}
      {mode === 'vr' && (
        <div className="absolute top-18 right-4 pointer-events-auto bg-white/95 backdrop-blur-md border border-slate-200 rounded-2xl p-3 shadow-xl flex flex-col items-center gap-1.5 z-20">
          <span className="text-[10px] font-bold text-slate-500 uppercase tracking-wider mb-0.5">D-Pad Walk</span>
          <button
            onMouseDown={() => { moveStateRef.current.forward = 1; }}
            onMouseUp={() => { moveStateRef.current.forward = 0; }}
            onTouchStart={() => { moveStateRef.current.forward = 1; }}
            onTouchEnd={() => { moveStateRef.current.forward = 0; }}
            className="w-8 h-8 rounded-xl bg-slate-100 hover:bg-red-600 hover:text-white border border-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs shadow-xs active:scale-95 transition"
            title="Step Forward (W / Up)"
          >
            ▲
          </button>
          <div className="flex items-center gap-1.5">
            <button
              onMouseDown={() => { moveStateRef.current.right = -1; }}
              onMouseUp={() => { moveStateRef.current.right = 0; }}
              onTouchStart={() => { moveStateRef.current.right = -1; }}
              onTouchEnd={() => { moveStateRef.current.right = 0; }}
              className="w-8 h-8 rounded-xl bg-slate-100 hover:bg-red-600 hover:text-white border border-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs shadow-xs active:scale-95 transition"
              title="Strafe Left (A / Left)"
            >
              ◀
            </button>
            <button
              onMouseDown={() => { moveStateRef.current.forward = -1; }}
              onMouseUp={() => { moveStateRef.current.forward = 0; }}
              onTouchStart={() => { moveStateRef.current.forward = -1; }}
              onTouchEnd={() => { moveStateRef.current.forward = 0; }}
              className="w-8 h-8 rounded-xl bg-slate-100 hover:bg-red-600 hover:text-white border border-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs shadow-xs active:scale-95 transition"
              title="Step Backward (S / Down)"
            >
              ▼
            </button>
            <button
              onMouseDown={() => { moveStateRef.current.right = 1; }}
              onMouseUp={() => { moveStateRef.current.right = 0; }}
              onTouchStart={() => { moveStateRef.current.right = 1; }}
              onTouchEnd={() => { moveStateRef.current.right = 0; }}
              className="w-8 h-8 rounded-xl bg-slate-100 hover:bg-red-600 hover:text-white border border-slate-200 text-slate-700 flex items-center justify-center font-bold text-xs shadow-xs active:scale-95 transition"
              title="Strafe Right (D / Right)"
            >
              ▶
            </button>
          </div>
        </div>
      )}

      {/* Bottom Info & Controller Overlay */}
      <div className="absolute bottom-4 left-4 right-4 flex items-end justify-between pointer-events-none z-20">
        {mode === 'vr' ? (
          <div className="pointer-events-auto bg-white/95 backdrop-blur-md border border-slate-200 rounded-2xl p-4 max-w-md shadow-xl text-slate-800">
            <div className="flex items-center gap-2 mb-2">
              <Glasses className="w-5 h-5 text-red-600" />
              <h4 className="font-bold text-sm text-slate-900">1:1 Eye-Level VR Walkthrough (1.7m Elevation)</h4>
            </div>
            <p className="text-xs text-slate-600 mb-3 leading-relaxed">
              Explore your building at true human perspective. Use keyboard <kbd className="px-1.5 py-0.5 bg-slate-100 rounded border border-slate-200 font-mono text-[11px] text-slate-800">W</kbd> <kbd className="px-1.5 py-0.5 bg-slate-100 rounded border border-slate-200 font-mono text-[11px] text-slate-800">A</kbd> <kbd className="px-1.5 py-0.5 bg-slate-100 rounded border border-slate-200 font-mono text-[11px] text-slate-800">S</kbd> <kbd className="px-1.5 py-0.5 bg-slate-100 rounded border border-slate-200 font-mono text-[11px] text-slate-800">D</kbd> or the D-Pad above, and drag mouse/touch to look around.
            </p>
            {xrSupported && (
              <button
                onClick={() => alert("Launching WebXR VR Session on headset...")}
                className="w-full bg-red-600 hover:bg-red-700 text-white py-2 rounded-xl text-xs font-semibold shadow-md transition"
              >
                Enter VR Headset Session
              </button>
            )}
          </div>
        ) : (
          <div className="pointer-events-auto bg-white/95 backdrop-blur-md border border-slate-200 rounded-2xl p-4 max-w-md shadow-xl text-slate-800">
            <div className="flex items-center gap-2 mb-2">
              <Smartphone className="w-5 h-5 text-emerald-600" />
              <h4 className="font-bold text-sm text-slate-900">Augmented Reality Surface Projection</h4>
            </div>
            <div className="space-y-3 text-xs text-slate-700">
              <div>
                <label className="text-slate-500 font-medium block mb-1">
                  Scale Mode: {arScale === 0.05 ? '1:20 Miniature Tabletop (0.05x)' : `${arScale.toFixed(2)}x Real Life Scale`}
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => setArScale(0.05)}
                    className={`flex-1 py-1.5 rounded-xl border text-xs font-semibold transition ${
                      arScale === 0.05 ? 'bg-emerald-600 border-emerald-500 text-white shadow-sm' : 'bg-slate-100 border-slate-200 text-slate-700 hover:bg-slate-200'
                    }`}
                  >
                    1:20 Tabletop Miniature
                  </button>
                  <button
                    onClick={() => setArScale(1.0)}
                    className={`flex-1 py-1.5 rounded-xl border text-xs font-semibold transition ${
                      arScale === 1.0 ? 'bg-emerald-600 border-emerald-500 text-white shadow-sm' : 'bg-slate-100 border-slate-200 text-slate-700 hover:bg-slate-200'
                    }`}
                  >
                    1:1 Site Scale
                  </button>
                </div>
              </div>

              <div>
                <label className="text-slate-500 font-medium block mb-1">
                  Placement Orientation (Y-Axis Rotation: {Math.round(rotationY * 57.2958)}°)
                </label>
                <input
                  type="range"
                  min="0"
                  max="6.28"
                  step="0.05"
                  value={rotationY}
                  onChange={(e) => setRotationY(parseFloat(e.target.value))}
                  className="w-full accent-emerald-600 cursor-pointer"
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
