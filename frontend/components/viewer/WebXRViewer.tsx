'use client';

import React, { useEffect, useState, useRef } from 'react';
import * as THREE from 'three';
import { BIMElement } from '../../types';
import { Compass, Glasses, Smartphone, Eye, CheckCircle2, AlertTriangle, ArrowLeft } from 'lucide-react';

interface WebXRViewerProps {
  mode: 'vr' | 'ar';
  elements: BIMElement[];
  onBack: () => void;
}

export default function WebXRViewer({ mode, elements, onBack }: WebXRViewerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [xrSupported, setXrSupported] = useState<boolean | null>(null);
  const [isSessionActive, setIsSessionActive] = useState(false);
  const [arScale, setArScale] = useState<number>(0.05); // 1:20 scale miniature default
  const [rotationY, setRotationY] = useState<number>(0);
  const [walkthroughActive, setWalkthroughActive] = useState(false);

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

    // Build building elements
    elements.forEach((elem) => {
      const w = elem.width || elem.thickness || 0.23;
      const h = elem.height || 3.0;
      const l = elem.length || 4.0;
      const geo = new THREE.BoxGeometry(w, h, l);

      let color = 0xd1d5db;
      if (elem.type === 'Floor') color = 0x94a3b8;
      if (elem.type === 'Door') color = 0xb45309;
      if (elem.type === 'Window') color = 0x38bdf8;
      if (elem.type === 'Roof') color = 0x475569;

      const mat = new THREE.MeshStandardMaterial({
        color,
        transparent: elem.type === 'Window',
        opacity: elem.type === 'Window' ? 0.45 : 1.0,
        roughness: 0.5
      });

      const mesh = new THREE.Mesh(geo, mat);
      mesh.position.set(elem.position[0], elem.position[1], elem.position[2]);
      houseGroup.add(mesh);
    });

    // Walkthrough controls (WASD + Mouse Drag)
    const moveState = { forward: 0, right: 0 };
    let isMouseDown = false;
    let yaw = 0;
    let pitch = 0;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'w' || e.key === 'ArrowUp') moveState.forward = 1;
      if (e.key === 's' || e.key === 'ArrowDown') moveState.forward = -1;
      if (e.key === 'a' || e.key === 'ArrowLeft') moveState.right = -1;
      if (e.key === 'd' || e.key === 'ArrowRight') moveState.right = 1;
    };

    const onKeyUp = (e: KeyboardEvent) => {
      if (['w', 's', 'ArrowUp', 'ArrowDown'].includes(e.key)) moveState.forward = 0;
      if (['a', 'd', 'ArrowLeft', 'ArrowRight'].includes(e.key)) moveState.right = 0;
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
      if (moveState.forward !== 0 || moveState.right !== 0) {
        const speed = 0.08;
        const forwardVector = new THREE.Vector3(Math.sin(yaw), 0, Math.cos(yaw)).normalize();
        const rightVector = new THREE.Vector3(Math.cos(yaw), 0, -Math.sin(yaw)).normalize();

        camera.position.addScaledVector(forwardVector, -moveState.forward * speed);
        camera.position.addScaledVector(rightVector, moveState.right * speed);
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
    <div className="relative w-full h-[640px] rounded-xl overflow-hidden border border-slate-700 bg-slate-950 flex flex-col shadow-2xl">
      {/* Viewport canvas */}
      <div ref={containerRef} className="w-full h-full cursor-crosshair" />

      {/* Top Banner with Device Capability Report */}
      <div className="absolute top-4 left-4 right-4 flex items-center justify-between pointer-events-none">
        <button
          onClick={onBack}
          className="pointer-events-auto bg-slate-900/90 hover:bg-slate-800 text-white px-3 py-2 rounded-lg border border-slate-700 text-xs font-semibold flex items-center gap-1.5 shadow"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to 3D Viewer
        </button>

        <div className="pointer-events-auto bg-slate-900/90 backdrop-blur-md px-4 py-2 rounded-lg border border-slate-700 text-xs text-slate-300 flex items-center gap-2 shadow">
          {xrSupported === true ? (
            <span className="flex items-center gap-1.5 text-emerald-400 font-medium">
              <CheckCircle2 className="w-4 h-4" />
              WebXR {mode.toUpperCase()} Supported
            </span>
          ) : (
            <span className="flex items-center gap-1.5 text-amber-400 font-medium">
              <AlertTriangle className="w-4 h-4" />
              WebXR {mode.toUpperCase()} Hardware Unavailable • Active Fallback Mode
            </span>
          )}
        </div>
      </div>

      {/* Controls Overlay */}
      <div className="absolute bottom-4 left-4 right-4 flex items-end justify-between pointer-events-none">
        {mode === 'vr' ? (
          <div className="pointer-events-auto bg-slate-900/95 backdrop-blur-md border border-slate-700 rounded-xl p-4 max-w-md shadow-2xl">
            <div className="flex items-center gap-2 mb-2">
              <Glasses className="w-5 h-5 text-red-500" />
              <h4 className="font-semibold text-sm text-white">First-Person VR Walkthrough</h4>
            </div>
            <p className="text-xs text-slate-400 mb-3 leading-relaxed">
              Eye-level immersive exploration at 1.7m human scale. Use <kbd className="px-1.5 py-0.5 bg-slate-800 rounded border border-slate-600 text-slate-200">W</kbd> <kbd className="px-1.5 py-0.5 bg-slate-800 rounded border border-slate-600 text-slate-200">A</kbd> <kbd className="px-1.5 py-0.5 bg-slate-800 rounded border border-slate-600 text-slate-200">S</kbd> <kbd className="px-1.5 py-0.5 bg-slate-800 rounded border border-slate-600 text-slate-200">D</kbd> or arrow keys to walk through doors, and click & drag to look around.
            </p>
            {xrSupported && (
              <button
                onClick={() => alert("Launching WebXR VR Session on headset...")}
                className="w-full bg-red-600 hover:bg-red-500 text-white py-2 rounded-lg text-xs font-semibold shadow transition"
              >
                Enter VR Headset Session
              </button>
            )}
          </div>
        ) : (
          <div className="pointer-events-auto bg-slate-900/95 backdrop-blur-md border border-slate-700 rounded-xl p-4 max-w-md shadow-2xl">
            <div className="flex items-center gap-2 mb-2">
              <Smartphone className="w-5 h-5 text-emerald-400" />
              <h4 className="font-semibold text-sm text-white">Augmented Reality Surface Placement</h4>
            </div>
            <div className="space-y-3 text-xs text-slate-300">
              <div>
                <label className="text-slate-400 block mb-1">
                  Scale Mode: {arScale === 0.05 ? '1:20 Miniature Tabletop (0.05x)' : `${arScale.toFixed(2)}x Real Scale`}
                </label>
                <div className="flex gap-2">
                  <button
                    onClick={() => setArScale(0.05)}
                    className={`flex-1 py-1.5 rounded-lg border text-xs font-medium transition ${
                      arScale === 0.05 ? 'bg-emerald-600 border-emerald-500 text-white' : 'bg-slate-800 border-slate-700 text-slate-300'
                    }`}
                  >
                    1:20 Miniature
                  </button>
                  <button
                    onClick={() => setArScale(1.0)}
                    className={`flex-1 py-1.5 rounded-lg border text-xs font-medium transition ${
                      arScale === 1.0 ? 'bg-emerald-600 border-emerald-500 text-white' : 'bg-slate-800 border-slate-700 text-slate-300'
                    }`}
                  >
                    1:1 Full Scale
                  </button>
                </div>
              </div>

              <div>
                <label className="text-slate-400 block mb-1">Rotate Placement (Y-Axis)</label>
                <input
                  type="range"
                  min="0"
                  max="6.28"
                  step="0.05"
                  value={rotationY}
                  onChange={(e) => setRotationY(parseFloat(e.target.value))}
                  className="w-full accent-emerald-500"
                />
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
