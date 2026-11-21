'use client';

import React, { useEffect, useRef, useState, useCallback } from 'react';
import * as THREE from 'three';
import { BIMElement, PotItem, BuildingCustomization } from '../../types';
import { 
  Eye, 
  Layers, 
  RotateCcw, 
  Box, 
  X, 
  Video, 
  StopCircle, 
  Palette, 
  DoorOpen, 
  Shield, 
  Flower2, 
  Plus, 
  Trash2, 
  Check, 
  Sparkles, 
  Sliders, 
  Move, 
  RefreshCw,
  SlidersHorizontal,
  ChevronRight,
  Maximize2
} from 'lucide-react';

interface BIM3DViewerProps {
  elements: BIMElement[];
  builtupArea: number;
  floorsCount: number;
  projectId?: string;
  onCustomizationChange?: (customization: BuildingCustomization) => void;
}

// Curated Wall Paint Presets
const WALL_PAINT_PRESETS = [
  { name: 'Classic White', hex: '#f8fafc', category: 'Neutral' },
  { name: 'Warm Ivory', hex: '#fef3c7', category: 'Warm' },
  { name: 'Desert Sand', hex: '#fde68a', category: 'Warm' },
  { name: 'Terracotta Earth', hex: '#c25e36', category: 'Accent' },
  { name: 'Soft Sage', hex: '#86efac', category: 'Nature' },
  { name: 'Forest Green', hex: '#15803d', category: 'Nature' },
  { name: 'Scandinavian Slate', hex: '#64748b', category: 'Modern' },
  { name: 'Modern Charcoal', hex: '#1e293b', category: 'Modern' },
  { name: 'Royal Navy', hex: '#1e40af', category: 'Accent' },
  { name: 'Warm Peach', hex: '#fed7aa', category: 'Warm' },
  { name: 'Crimson Accent', hex: '#991b1b', category: 'Accent' },
  { name: 'Lavender Mist', hex: '#ddd6fe', category: 'Pastel' },
];

// Curated Door Finish Presets
const DOOR_COLOR_PRESETS = [
  { name: 'Natural Teak', hex: '#b45309' },
  { name: 'Dark Walnut', hex: '#451a03' },
  { name: 'Modern Charcoal', hex: '#1f2937' },
  { name: 'Classic White', hex: '#f8fafc' },
  { name: 'Mahogany Red', hex: '#7f1d1d' },
  { name: 'Midnight Navy', hex: '#172554' },
  { name: 'Golden Oak', hex: '#d97706' },
  { name: 'Forest Green', hex: '#14532d' },
  { name: 'Slate Grey', hex: '#475569' },
];

// Curated Grill Presets
const GRILL_COLOR_PRESETS = [
  { name: 'Matte Black', hex: '#18181b' },
  { name: 'Wrought Iron Charcoal', hex: '#27272a' },
  { name: 'Brushed Silver', hex: '#cbd5e1' },
  { name: 'Antique Bronze', hex: '#78350f' },
  { name: 'Golden Brass', hex: '#ca8a04' },
  { name: 'Architectural White', hex: '#ffffff' },
];

// Curated Pot Materials
const POT_COLOR_PRESETS = [
  { name: 'Terracotta Clay', hex: '#c25e36' },
  { name: 'Glazed White Ceramic', hex: '#f8fafc' },
  { name: 'Basalt Slate Charcoal', hex: '#334155' },
  { name: 'Brass Gold', hex: '#ca8a04' },
  { name: 'Forest Green Glaze', hex: '#15803d' },
  { name: 'Cobalt Blue', hex: '#1d4ed8' },
];

export default function BIM3DViewer({ 
  elements, 
  builtupArea, 
  floorsCount, 
  projectId = 'default',
  onCustomizationChange 
}: BIM3DViewerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [selectedElement, setSelectedElement] = useState<BIMElement | null>(null);
  const [selectedPot, setSelectedPot] = useState<PotItem | null>(null);
  const [showCeilings, setShowCeilings] = useState(false);
  const [showRoof, setShowRoof] = useState(true);
  const [isRecording, setIsRecording] = useState(false);
  const [recordSeconds, setRecordSeconds] = useState(0);

  // Customization Sidebar State
  const [showCustomizer, setShowCustomizer] = useState(false);
  const [activeTab, setActiveTab] = useState<'wall' | 'door' | 'grill' | 'pots'>('wall');
  const [clickToPlacePot, setClickToPlacePot] = useState(false);
  const [pendingPotType, setPendingPotType] = useState<PotItem['type']>('terracotta');

  // Building Customization State
  const [wallColor, setWallColor] = useState('#d1d5db');
  const [wallFinish, setWallFinish] = useState<'matte' | 'satin' | 'gloss'>('satin');
  const [customWallColors, setCustomWallColors] = useState<Record<string, string>>({});
  
  const [doorColor, setDoorColor] = useState('#b45309');
  const [customDoorColors, setCustomDoorColors] = useState<Record<string, string>>({});

  const [grillColor, setGrillColor] = useState('#18181b');
  const [showGrills, setShowGrills] = useState(true);
  const [grillStyle, setGrillStyle] = useState<'vertical' | 'grid' | 'modern'>('vertical');

  const [pots, setPots] = useState<PotItem[]>([]);

  // Three.js References
  const sceneRef = useRef<THREE.Scene | null>(null);
  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);
  const cameraRef = useRef<THREE.PerspectiveCamera | null>(null);
  const meshMapRef = useRef<Map<string, { mesh: THREE.Mesh; element: BIMElement }>>(new Map());
  const selectedMeshRef = useRef<THREE.Mesh | null>(null);
  const grillsGroupRef = useRef<THREE.Group | null>(null);
  const potsGroupRef = useRef<THREE.Group | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const raycasterRef = useRef(new THREE.Raycaster());
  const mouseRef = useRef(new THREE.Vector2());

  // Load saved customization from localStorage
  useEffect(() => {
    try {
      const savedKey = `smartbuild_customization_${projectId}`;
      const saved = localStorage.getItem(savedKey);
      if (saved) {
        const parsed: BuildingCustomization = JSON.parse(saved);
        if (parsed.wallColor) setWallColor(parsed.wallColor);
        if (parsed.wallFinish) setWallFinish(parsed.wallFinish);
        if (parsed.customWallColors) setCustomWallColors(parsed.customWallColors);
        if (parsed.doorColor) setDoorColor(parsed.doorColor);
        if (parsed.customDoorColors) setCustomDoorColors(parsed.customDoorColors);
        if (parsed.grillColor) setGrillColor(parsed.grillColor);
        if (parsed.showGrills !== undefined) setShowGrills(parsed.showGrills);
        if (parsed.grillStyle) setGrillStyle(parsed.grillStyle);
        if (parsed.pots && parsed.pots.length > 0) {
          setPots(parsed.pots);
        } else {
          // Initialize with twin entrance pots by default
          initializeDefaultPots();
        }
      } else {
        initializeDefaultPots();
      }
    } catch {
      initializeDefaultPots();
    }
  }, [projectId]);

  // Save customization when it changes
  useEffect(() => {
    try {
      const custom: BuildingCustomization = {
        wallColor,
        wallFinish,
        customWallColors,
        doorColor,
        customDoorColors,
        grillColor,
        showGrills,
        grillStyle,
        pots,
      };
      localStorage.setItem(`smartbuild_customization_${projectId}`, JSON.stringify(custom));
      if (onCustomizationChange) {
        onCustomizationChange(custom);
      }
    } catch (e) {
      console.warn('Could not persist 3D customization:', e);
    }
  }, [wallColor, wallFinish, customWallColors, doorColor, customDoorColors, grillColor, showGrills, grillStyle, pots, projectId]);

  // Helper to place default decorative pots near front door / porch
  const initializeDefaultPots = () => {
    // Locate the first door or front area to place twin welcoming pots
    const doorElem = elements.find(e => e.type === 'Door');
    const basePos = doorElem ? doorElem.position : [2.0, 0, 0];
    
    const defaultEntrancePots: PotItem[] = [
      {
        id: 'pot_init_1',
        name: 'Entrance Terracotta Pot (Left)',
        type: 'terracotta',
        plantType: 'foliage',
        potColor: '#c25e36',
        position: [basePos[0] - 0.7, 0, basePos[2] - 0.4],
        scale: 1.0,
      },
      {
        id: 'pot_init_2',
        name: 'Entrance Terracotta Pot (Right)',
        type: 'terracotta',
        plantType: 'foliage',
        potColor: '#c25e36',
        position: [basePos[0] + 0.7, 0, basePos[2] - 0.4],
        scale: 1.0,
      }
    ];
    setPots(defaultEntrancePots);
  };

  // -------------------------------------------------------------
  // Procedural 3D Pot Generator
  // -------------------------------------------------------------
  const createPotMeshGroup = useCallback((pot: PotItem): THREE.Group => {
    const group = new THREE.Group();
    group.name = `Pot_${pot.id}`;
    group.position.set(pot.position[0], pot.position[1], pot.position[2]);
    group.scale.setScalar(pot.scale || 1.0);

    const potColorNum = new THREE.Color(pot.potColor || '#c25e36').getHex();
    const isCeramic = pot.type === 'ceramic';
    const potMat = new THREE.MeshStandardMaterial({
      color: potColorNum,
      roughness: isCeramic ? 0.25 : 0.7,
      metalness: isCeramic ? 0.15 : 0.05,
    });

    let potHeight = 0.52;
    let topR = 0.26;
    let botR = 0.18;

    if (pot.type === 'ceramic') {
      potHeight = 0.72;
      topR = 0.22;
      botR = 0.16;
    } else if (pot.type === 'urn') {
      potHeight = 0.78;
      topR = 0.34;
      botR = 0.22;
    } else if (pot.type === 'flower') {
      potHeight = 0.44;
      topR = 0.28;
      botR = 0.20;
    } else if (pot.type === 'bonsai') {
      potHeight = 0.24;
      topR = 0.36;
      botR = 0.30;
    }

    // 1. Pot Body (tapered cylinder)
    const potGeo = new THREE.CylinderGeometry(topR, botR, potHeight, 18);
    const potMesh = new THREE.Mesh(potGeo, potMat);
    potMesh.position.y = potHeight / 2;
    potMesh.castShadow = true;
    potMesh.receiveShadow = true;
    potMesh.userData = { isPot: true, potData: pot };
    group.add(potMesh);

    // 2. Rolled Top Rim
    const rimGeo = new THREE.TorusGeometry(topR, 0.032, 8, 20);
    rimGeo.rotateX(Math.PI / 2);
    const rimMesh = new THREE.Mesh(rimGeo, potMat);
    rimMesh.position.y = potHeight;
    rimMesh.castShadow = true;
    rimMesh.userData = { isPot: true, potData: pot };
    group.add(rimMesh);

    // 3. Potting Soil
    const soilGeo = new THREE.CylinderGeometry(topR * 0.94, topR * 0.94, 0.05, 16);
    const soilMat = new THREE.MeshStandardMaterial({ color: 0x2b180d, roughness: 0.95 });
    const soilMesh = new THREE.Mesh(soilGeo, soilMat);
    soilMesh.position.y = potHeight - 0.015;
    group.add(soilMesh);

    // 4. Plant Foliage
    const leafMatDark = new THREE.MeshStandardMaterial({ color: 0x15803d, roughness: 0.6 });
    const leafMatLight = new THREE.MeshStandardMaterial({ color: 0x22c55e, roughness: 0.5 });
    const flowerMat = new THREE.MeshStandardMaterial({ color: 0xf43f5e, roughness: 0.4 });
    const flowerMatYellow = new THREE.MeshStandardMaterial({ color: 0xfacc15, roughness: 0.4 });

    if (pot.plantType === 'snake') {
      // Upright architectural snake plant blades
      const bladeCount = 9;
      for (let i = 0; i < bladeCount; i++) {
        const angle = (i / bladeCount) * Math.PI * 2;
        const bladeH = 0.55 + (i % 3) * 0.18;
        const bladeGeo = new THREE.ConeGeometry(0.065, bladeH, 4);
        const bladeMat = i % 2 === 0 ? leafMatDark : leafMatLight;
        const blade = new THREE.Mesh(bladeGeo, bladeMat);
        blade.position.set(
          Math.cos(angle) * (topR * 0.42),
          potHeight + bladeH / 2,
          Math.sin(angle) * (topR * 0.42)
        );
        blade.rotation.z = Math.cos(angle) * 0.08;
        blade.rotation.x = Math.sin(angle) * 0.08;
        blade.castShadow = true;
        group.add(blade);
      }
    } else if (pot.plantType === 'palm') {
      // Tropical palm fronds radiating outward
      const frondCount = 8;
      for (let i = 0; i < frondCount; i++) {
        const angle = (i / frondCount) * Math.PI * 2;
        const frondGeo = new THREE.BoxGeometry(0.07, 0.65, 0.018);
        const frond = new THREE.Mesh(frondGeo, leafMatDark);
        frond.position.set(
          Math.cos(angle) * (topR * 0.45),
          potHeight + 0.3,
          Math.sin(angle) * (topR * 0.45)
        );
        frond.rotation.y = angle;
        frond.rotation.x = 0.45;
        frond.castShadow = true;
        group.add(frond);
      }
    } else if (pot.plantType === 'flowers') {
      // Flowering bush with blossoms
      const bushGeo = new THREE.DodecahedronGeometry(0.26, 1);
      const bush = new THREE.Mesh(bushGeo, leafMatDark);
      bush.position.y = potHeight + 0.22;
      bush.castShadow = true;
      group.add(bush);

      const blossomCount = 8;
      for (let i = 0; i < blossomCount; i++) {
        const phi = Math.acos(-1 + (2 * i) / blossomCount);
        const theta = Math.sqrt(blossomCount * Math.PI) * phi;
        const bGeo = new THREE.SphereGeometry(0.055, 8, 8);
        const bMat = i % 2 === 0 ? flowerMat : flowerMatYellow;
        const blossom = new THREE.Mesh(bGeo, bMat);
        blossom.position.set(
          Math.cos(theta) * Math.sin(phi) * 0.27,
          potHeight + 0.22 + Math.cos(phi) * 0.18,
          Math.sin(theta) * Math.sin(phi) * 0.27
        );
        group.add(blossom);
      }
    } else {
      // Standard lush garden foliage
      const domeGeo = new THREE.SphereGeometry(0.26, 14, 10);
      const dome = new THREE.Mesh(domeGeo, leafMatDark);
      dome.position.y = potHeight + 0.24;
      dome.scale.set(1.15, 0.9, 1.15);
      dome.castShadow = true;
      group.add(dome);

      const subFoliageGeo = new THREE.SphereGeometry(0.19, 10, 8);
      const subFoliage = new THREE.Mesh(subFoliageGeo, leafMatLight);
      subFoliage.position.set(0.08, potHeight + 0.34, -0.06);
      subFoliage.castShadow = true;
      group.add(subFoliage);
    }

    group.userData = { isPot: true, potData: pot };
    return group;
  }, []);

  // -------------------------------------------------------------
  // Procedural 3D Window Grill Generator
  // -------------------------------------------------------------
  const createWindowGrillGroup = useCallback((
    windowElem: BIMElement, 
    colorHex: string, 
    style: 'vertical' | 'grid' | 'modern'
  ): THREE.Group => {
    const grillGroup = new THREE.Group();
    grillGroup.name = `Grill_${windowElem.id}`;

    const h = windowElem.height || 1.2;
    const l = windowElem.length || 1.5;
    const barThick = 0.016;

    const grillMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color(colorHex).getHex(),
      metalness: 0.85,
      roughness: 0.25,
    });

    // 1. Frame perimeter
    const topBar = new THREE.Mesh(new THREE.BoxGeometry(barThick * 1.5, barThick * 1.5, l), grillMat);
    topBar.position.set(0, h / 2 - barThick, 0);
    grillGroup.add(topBar);

    const botBar = new THREE.Mesh(new THREE.BoxGeometry(barThick * 1.5, barThick * 1.5, l), grillMat);
    botBar.position.set(0, -h / 2 + barThick, 0);
    grillGroup.add(botBar);

    const leftBar = new THREE.Mesh(new THREE.BoxGeometry(barThick * 1.5, h, barThick * 1.5), grillMat);
    leftBar.position.set(0, 0, -l / 2 + barThick);
    grillGroup.add(leftBar);

    const rightBar = new THREE.Mesh(new THREE.BoxGeometry(barThick * 1.5, h, barThick * 1.5), grillMat);
    rightBar.position.set(0, 0, l / 2 - barThick);
    grillGroup.add(rightBar);

    // 2. Mid horizontal bar
    const midBar = new THREE.Mesh(new THREE.BoxGeometry(barThick * 1.2, barThick * 1.2, l - 0.02), grillMat);
    midBar.position.set(0, 0, 0);
    grillGroup.add(midBar);

    // 3. Infill Pattern according to style
    if (style === 'vertical' || style === 'grid') {
      const numBars = Math.max(3, Math.floor(l / 0.18));
      const step = (l - 0.1) / (numBars + 1);
      for (let i = 1; i <= numBars; i++) {
        const vBar = new THREE.Mesh(new THREE.CylinderGeometry(0.007, 0.007, h - 0.04, 8), grillMat);
        vBar.position.set(0, 0, -l / 2 + 0.05 + i * step);
        vBar.castShadow = true;
        grillGroup.add(vBar);
      }
    }

    if (style === 'grid' || style === 'modern') {
      [-h / 4, h / 4].forEach(yPos => {
        const hBar = new THREE.Mesh(new THREE.BoxGeometry(barThick, barThick, l - 0.04), grillMat);
        hBar.position.set(0, yPos, 0);
        grillGroup.add(hBar);
      });
    }

    // Position grill right on the window opening plane, slightly forward
    grillGroup.position.set(
      windowElem.position[0],
      windowElem.position[1],
      windowElem.position[2]
    );

    grillGroup.userData = { isGrill: true, bimElement: windowElem };
    return grillGroup;
  }, []);

  // -------------------------------------------------------------
  // Primary Three.js Scene Setup & Initialization
  // -------------------------------------------------------------
  useEffect(() => {
    if (!containerRef.current) return;

    // 1. Scene
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0f172a); // Slate-900
    sceneRef.current = scene;

    // 2. Camera
    const width = containerRef.current.clientWidth || 800;
    const height = containerRef.current.clientHeight || 550;
    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    camera.position.set(16, 14, 20);
    camera.lookAt(6, 2, 6);
    cameraRef.current = camera;

    // 3. Renderer
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true, powerPreference: 'high-performance' });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;

    containerRef.current.innerHTML = '';
    containerRef.current.appendChild(renderer.domElement);
    rendererRef.current = renderer;

    // 4. Lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.75);
    scene.add(ambientLight);

    const dirLight = new THREE.DirectionalLight(0xffffff, 0.95);
    dirLight.position.set(22, 36, 22);
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

    // 6. Build BIM Meshes
    meshMapRef.current.clear();
    const roughnessByFinish = wallFinish === 'matte' ? 0.85 : wallFinish === 'gloss' ? 0.2 : 0.5;
    const metalnessByFinish = wallFinish === 'gloss' ? 0.1 : 0.02;

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
        geo = new THREE.BoxGeometry(0.08, h, l);
      } else if (elem.type === 'Window') {
        geo = new THREE.BoxGeometry(0.06, h, l);
      } else {
        geo = new THREE.BoxGeometry(w, h, l);
      }

      // Material color determination
      let matColor = 0xd1d5db;
      let matRoughness = 0.5;
      let matMetalness = 0.1;
      let isGlass = false;

      if (elem.type === 'Wall') {
        const customCol = customWallColors[elem.id] || wallColor;
        matColor = new THREE.Color(customCol).getHex();
        matRoughness = roughnessByFinish;
        matMetalness = metalnessByFinish;
      } else if (elem.type === 'Door') {
        const customDoor = customDoorColors[elem.id] || doorColor;
        matColor = new THREE.Color(customDoor).getHex();
        matRoughness = 0.45;
        matMetalness = 0.05;
      } else if (elem.type === 'Floor') {
        matColor = 0x94a3b8;
        matRoughness = 0.4;
      } else if (elem.type === 'Roof') {
        matColor = 0x475569;
        matRoughness = 0.7;
      } else if (elem.type === 'Ceiling') {
        matColor = 0xf1f5f9;
        matRoughness = 0.9;
      } else if (elem.type === 'Window') {
        matColor = 0x38bdf8;
        isGlass = true;
      }

      let mat: THREE.Material;
      if (isGlass) {
        mat = new THREE.MeshPhysicalMaterial({
          color: matColor,
          transparent: true,
          opacity: 0.4,
          roughness: 0.1,
          transmission: 0.7,
        });
      } else {
        mat = new THREE.MeshStandardMaterial({
          color: matColor,
          roughness: matRoughness,
          metalness: matMetalness,
        });
      }

      const mesh = new THREE.Mesh(geo, mat);
      mesh.position.set(elem.position[0], elem.position[1], elem.position[2]);
      mesh.castShadow = true;
      mesh.receiveShadow = true;
      mesh.userData = { bimElement: elem };

      // Hide ceilings by default
      if (elem.type === 'Ceiling') {
        mesh.visible = showCeilings;
      }
      if (elem.type === 'Roof') {
        mesh.visible = showRoof;
      }

      // If door, add a stylish metallic door handle
      if (elem.type === 'Door') {
        const handleGeo = new THREE.CylinderGeometry(0.015, 0.015, 0.14, 8);
        const handleMat = new THREE.MeshStandardMaterial({ color: 0xe2e8f0, metalness: 0.9, roughness: 0.2 });
        const handleMesh = new THREE.Mesh(handleGeo, handleMat);
        handleMesh.position.set(0.05, 0, l * 0.35);
        mesh.add(handleMesh);
      }

      scene.add(mesh);
      meshMapRef.current.set(elem.id, { mesh, element: elem });
    });

    // 7. Window Grills Group
    const grillsGroup = new THREE.Group();
    grillsGroup.name = 'WindowGrills';
    scene.add(grillsGroup);
    grillsGroupRef.current = grillsGroup;

    if (showGrills) {
      elements.filter(e => e.type === 'Window').forEach(winElem => {
        const grill = createWindowGrillGroup(winElem, grillColor, grillStyle);
        grillsGroup.add(grill);
      });
    }

    // 8. Pots Group
    const potsGroup = new THREE.Group();
    potsGroup.name = 'PotsAndPlants';
    scene.add(potsGroup);
    potsGroupRef.current = potsGroup;

    pots.forEach(pot => {
      const potGroup = createPotMeshGroup(pot);
      potsGroup.add(potGroup);
    });

    // 9. Interactive Orbiting & Raycasting
    let isDragging = false;
    let prevMousePos = { x: 0, y: 0 };
    let spherical = new THREE.Spherical(26, Math.PI / 3.5, Math.PI / 4);
    let target = new THREE.Vector3(6, 2, 6);

    const updateCamera = () => {
      spherical.radius = Math.max(3, Math.min(65, spherical.radius));
      spherical.phi = Math.max(0.08, Math.min(Math.PI / 2.05, spherical.phi));
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
        spherical.theta -= dx * 0.008;
        spherical.phi -= dy * 0.008;
      } else if (e.buttons === 2) {
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

    // Click handler for selection or click-to-place pot
    const onClick = (e: MouseEvent) => {
      const rect = dom.getBoundingClientRect();
      mouseRef.current.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
      mouseRef.current.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

      raycasterRef.current.setFromCamera(mouseRef.current, camera);

      // Check if user is in "Click to Place Pot" mode
      if (clickToPlacePot) {
        // Raycast against floors and ground
        const floorMeshes = Array.from(meshMapRef.current.values())
          .map(v => v.mesh)
          .filter(m => m.visible && (m.userData?.bimElement?.type === 'Floor' || m.userData?.bimElement?.type === 'Roof'));

        // Also add an invisible ground plane for raycasting outside building
        const groundHits = raycasterRef.current.intersectObjects([grid, ...floorMeshes], false);
        if (groundHits.length > 0) {
          const pt = groundHits[0].point;
          const newPot: PotItem = {
            id: `pot_${Date.now()}_${Math.random().toString(36).substring(2, 6)}`,
            name: `${pendingPotType.charAt(0).toUpperCase() + pendingPotType.slice(1)} Planter #${pots.length + 1}`,
            type: pendingPotType,
            plantType: pendingPotType === 'ceramic' ? 'snake' : pendingPotType === 'flower' ? 'flowers' : 'foliage',
            potColor: pendingPotType === 'ceramic' ? '#f8fafc' : '#c25e36',
            position: [Math.round(pt.x * 100) / 100, Math.max(0, Math.round(pt.y * 100) / 100), Math.round(pt.z * 100) / 100],
            scale: 1.0,
          };
          setPots(prev => [...prev, newPot]);
          setSelectedPot(newPot);
          setClickToPlacePot(false);
          return;
        }
      }

      // Check pot raycasting first
      if (potsGroupRef.current) {
        const potHits = raycasterRef.current.intersectObjects(potsGroupRef.current.children, true);
        if (potHits.length > 0) {
          // Find topmost root pot group
          let cur: THREE.Object3D | null = potHits[0].object;
          while (cur && cur.parent && cur.parent !== potsGroupRef.current) {
            cur = cur.parent;
          }
          if (cur && cur.userData?.potData) {
            setSelectedPot(cur.userData.potData);
            setSelectedElement(null);
            setShowCustomizer(true);
            setActiveTab('pots');
            return;
          }
        }
      }

      // Check building BIM elements raycasting
      const meshes = Array.from(meshMapRef.current.values()).map(v => v.mesh).filter(m => m.visible);
      const intersects = raycasterRef.current.intersectObjects(meshes, false);

      if (intersects.length > 0) {
        const hit = intersects[0].object as THREE.Mesh;
        const elemData = hit.userData.bimElement as BIMElement;

        // Reset previous highlight
        if (selectedMeshRef.current && (selectedMeshRef.current.material as THREE.MeshStandardMaterial).emissive) {
          (selectedMeshRef.current.material as THREE.MeshStandardMaterial).emissive.setHex(0x000000);
        }

        // Highlight new selection with crimson glow
        selectedMeshRef.current = hit;
        if ((hit.material as THREE.MeshStandardMaterial).emissive) {
          (hit.material as THREE.MeshStandardMaterial).emissive.setHex(0xdc2626);
        }

        setSelectedElement(elemData);
        setSelectedPot(null);

        // Auto-switch customizer tab to relevant element
        if (elemData.type === 'Wall') {
          setActiveTab('wall');
        } else if (elemData.type === 'Door') {
          setActiveTab('door');
        } else if (elemData.type === 'Window') {
          setActiveTab('grill');
        }
      } else {
        // Deselect when clicking empty space
        if (selectedMeshRef.current && (selectedMeshRef.current.material as THREE.MeshStandardMaterial).emissive) {
          (selectedMeshRef.current.material as THREE.MeshStandardMaterial).emissive.setHex(0x000000);
        }
        selectedMeshRef.current = null;
        setSelectedElement(null);
        setSelectedPot(null);
      }
    };

    dom.addEventListener('mousedown', onMouseDown);
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    dom.addEventListener('wheel', onWheel, { passive: false });
    dom.addEventListener('click', onClick);
    dom.addEventListener('contextmenu', (e) => e.preventDefault());

    // 10. Render Loop
    let animId: number;
    const animate = () => {
      animId = requestAnimationFrame(animate);
      renderer.render(scene, camera);
    };
    animate();

    // 11. Resize Observer
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
  }, [elements, wallFinish]); // Re-render base scene on element changes

  // -------------------------------------------------------------
  // Live Updates for Wall Paint
  // -------------------------------------------------------------
  useEffect(() => {
    meshMapRef.current.forEach(({ mesh, element }) => {
      if (element.type === 'Wall') {
        const col = customWallColors[element.id] || wallColor;
        const mat = mesh.material as THREE.MeshStandardMaterial;
        mat.color.set(col);
        mat.roughness = wallFinish === 'matte' ? 0.85 : wallFinish === 'gloss' ? 0.2 : 0.5;
        mat.metalness = wallFinish === 'gloss' ? 0.1 : 0.02;
        mat.needsUpdate = true;
      }
    });
  }, [wallColor, wallFinish, customWallColors]);

  // -------------------------------------------------------------
  // Live Updates for Door Colors
  // -------------------------------------------------------------
  useEffect(() => {
    meshMapRef.current.forEach(({ mesh, element }) => {
      if (element.type === 'Door') {
        const col = customDoorColors[element.id] || doorColor;
        const mat = mesh.material as THREE.MeshStandardMaterial;
        mat.color.set(col);
        mat.roughness = 0.45;
        mat.needsUpdate = true;
      }
    });
  }, [doorColor, customDoorColors]);

  // -------------------------------------------------------------
  // Live Updates for Window Grills
  // -------------------------------------------------------------
  useEffect(() => {
    if (!grillsGroupRef.current) return;
    
    // Clear existing grills
    while (grillsGroupRef.current.children.length > 0) {
      grillsGroupRef.current.remove(grillsGroupRef.current.children[0]);
    }

    if (showGrills) {
      elements.filter(e => e.type === 'Window').forEach(winElem => {
        const grill = createWindowGrillGroup(winElem, grillColor, grillStyle);
        grillsGroupRef.current?.add(grill);
      });
    }
  }, [showGrills, grillColor, grillStyle, elements, createWindowGrillGroup]);

  // -------------------------------------------------------------
  // Live Updates for Pots
  // -------------------------------------------------------------
  useEffect(() => {
    if (!potsGroupRef.current) return;

    // Clear existing pots
    while (potsGroupRef.current.children.length > 0) {
      potsGroupRef.current.remove(potsGroupRef.current.children[0]);
    }

    // Rebuild pots
    pots.forEach(pot => {
      const potGroup = createPotMeshGroup(pot);
      potsGroupRef.current?.add(potGroup);
    });
  }, [pots, createPotMeshGroup]);

  // Visibility Toggles
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

  // Video recording
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

  // Add a new pot with preset position
  const handleAddPotPreset = (type: PotItem['type']) => {
    const door = elements.find(e => e.type === 'Door');
    const base = door ? door.position : [2.0, 0, 2.0];
    const offset = (pots.length % 4) * 0.7;

    const newPot: PotItem = {
      id: `pot_${Date.now()}_${Math.random().toString(36).substring(2, 6)}`,
      name: `${type.charAt(0).toUpperCase() + type.slice(1)} Planter #${pots.length + 1}`,
      type,
      plantType: type === 'ceramic' ? 'snake' : type === 'urn' ? 'palm' : type === 'flower' ? 'flowers' : 'foliage',
      potColor: type === 'ceramic' ? '#f8fafc' : type === 'urn' ? '#334155' : '#c25e36',
      position: [
        base[0] + 0.8 + offset, 
        0, 
        base[2] + (pots.length % 2 === 0 ? 0.6 : -0.6)
      ],
      scale: 1.0,
    };

    setPots(prev => [...prev, newPot]);
    setSelectedPot(newPot);
    setSelectedElement(null);
  };

  // Add symmetrical twin pots at front door
  const handleAddTwinPots = () => {
    const door = elements.find(e => e.type === 'Door');
    const base = door ? door.position : [2.0, 0, 2.0];

    const leftPot: PotItem = {
      id: `pot_twin_l_${Date.now()}`,
      name: 'Twin Entrance Pot (Left)',
      type: 'terracotta',
      plantType: 'foliage',
      potColor: '#c25e36',
      position: [base[0] - 0.75, 0, base[2] - 0.45],
      scale: 1.05,
    };

    const rightPot: PotItem = {
      id: `pot_twin_r_${Date.now()}`,
      name: 'Twin Entrance Pot (Right)',
      type: 'terracotta',
      plantType: 'foliage',
      potColor: '#c25e36',
      position: [base[0] + 0.75, 0, base[2] - 0.45],
      scale: 1.05,
    };

    setPots(prev => [...prev, leftPot, rightPot]);
    setSelectedPot(leftPot);
  };

  // Delete a pot
  const handleDeletePot = (id: string) => {
    setPots(prev => prev.filter(p => p.id !== id));
    if (selectedPot?.id === id) {
      setSelectedPot(null);
    }
  };

  // Update selected pot properties
  const handleUpdateSelectedPot = (field: keyof PotItem, value: any) => {
    if (!selectedPot) return;
    const updated = { ...selectedPot, [field]: value };
    setSelectedPot(updated);
    setPots(prev => prev.map(p => p.id === updated.id ? updated : p));
  };

  // Move selected pot
  const handleMovePot = (axis: 'x' | 'z', delta: number) => {
    if (!selectedPot) return;
    const newPos: [number, number, number] = [
      axis === 'x' ? selectedPot.position[0] + delta : selectedPot.position[0],
      selectedPot.position[1],
      axis === 'z' ? selectedPot.position[2] + delta : selectedPot.position[2],
    ];
    handleUpdateSelectedPot('position', newPos);
  };

  // Reset customizations to defaults
  const handleResetToDefaults = () => {
    if (!confirm('Reset all wall paints, door colors, grills, and pots to defaults?')) return;
    setWallColor('#d1d5db');
    setWallFinish('satin');
    setCustomWallColors({});
    setDoorColor('#b45309');
    setCustomDoorColors({});
    setGrillColor('#18181b');
    setShowGrills(true);
    setGrillStyle('vertical');
    initializeDefaultPots();
    setSelectedElement(null);
    setSelectedPot(null);
  };

  return (
    <div className="relative w-full h-[640px] rounded-xl overflow-hidden border border-slate-700 bg-slate-950 shadow-2xl flex flex-col select-none">
      {/* 3D Viewport canvas container */}
      <div 
        ref={containerRef} 
        className={`w-full h-full ${clickToPlacePot ? 'cursor-crosshair' : 'cursor-grab active:cursor-grabbing'}`} 
      />

      {/* Click to Place Pot Banner if active */}
      {clickToPlacePot && (
        <div className="absolute top-20 left-1/2 -translate-x-1/2 z-30 bg-emerald-600 text-white px-5 py-2.5 rounded-full shadow-2xl border border-emerald-400 text-xs font-semibold flex items-center gap-2 animate-bounce">
          <Flower2 className="w-4 h-4" />
          Click anywhere on the floor/ground to place your {pendingPotType} pot!
          <button 
            onClick={() => setClickToPlacePot(false)}
            className="ml-2 bg-emerald-800 hover:bg-emerald-900 px-2 py-0.5 rounded-full text-[10px]"
          >
            Cancel
          </button>
        </div>
      )}

      {/* Top Floating Controls Bar */}
      <div className="absolute top-4 left-4 right-4 flex items-center justify-between pointer-events-none z-20">
        <div className="bg-slate-900/90 backdrop-blur-md px-4 py-2 rounded-lg border border-slate-700 text-xs text-slate-300 pointer-events-auto flex items-center gap-3 shadow-lg">
          <span className="font-semibold text-red-500 flex items-center gap-1.5">
            <Box className="w-3.5 h-3.5" />
            3D BIM Studio
          </span>
          <span className="text-slate-500">|</span>
          <span>{elements.length} Components</span>
          <span className="text-slate-500">|</span>
          <span>{builtupArea} m²</span>
          <span className="text-slate-500">|</span>
          <span>{pots.length} {pots.length === 1 ? 'Pot' : 'Pots'}</span>
        </div>

        <div className="flex items-center gap-2 pointer-events-auto">
          {/* Main Customization Toggle Button */}
          <button
            onClick={() => setShowCustomizer(!showCustomizer)}
            className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all flex items-center gap-1.5 shadow-lg ${
              showCustomizer 
                ? 'bg-gradient-to-r from-red-600 to-rose-600 border-red-400 text-white shadow-red-600/40 ring-2 ring-red-500/50' 
                : 'bg-slate-800/95 border-slate-700 text-slate-200 hover:bg-slate-700 hover:text-white'
            }`}
            title="Open 3D Finishes, Wall Paint, Grill Color & Pots Customizer"
          >
            <Palette className="w-3.5 h-3.5 text-rose-400" />
            <span>Customize Finishes & Decor</span>
            <span className="w-2 h-2 rounded-full bg-rose-400 animate-ping" />
          </button>

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
            title="Reset Camera View"
          >
            <RotateCcw className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      {/* ========================================================= */}
      {/* 3D CUSTOMIZATION DRAWER PANEL                             */}
      {/* ========================================================= */}
      {showCustomizer && (
        <div className="absolute top-16 right-4 bottom-14 w-84 max-w-[340px] bg-slate-900/95 backdrop-blur-xl border border-slate-700 rounded-2xl shadow-2xl flex flex-col z-30 animate-in fade-in slide-in-from-right duration-200 overflow-hidden">
          {/* Header */}
          <div className="px-4 py-3 border-b border-slate-800 flex items-center justify-between bg-slate-950/60">
            <div className="flex items-center gap-2">
              <Sparkles className="w-4 h-4 text-rose-500" />
              <h2 className="text-xs font-bold text-white tracking-wide uppercase">3D Finishes & Decor</h2>
            </div>
            <button
              onClick={() => setShowCustomizer(false)}
              className="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          {/* Navigation Tabs */}
          <div className="flex items-center border-b border-slate-800 bg-slate-950/40 p-1 gap-1 text-[11px] font-semibold">
            <button
              onClick={() => setActiveTab('wall')}
              className={`flex-1 py-1.5 rounded-lg flex items-center justify-center gap-1 transition ${
                activeTab === 'wall' 
                  ? 'bg-rose-600 text-white shadow' 
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              <Palette className="w-3.5 h-3.5" />
              Wall Paint
            </button>

            <button
              onClick={() => setActiveTab('door')}
              className={`flex-1 py-1.5 rounded-lg flex items-center justify-center gap-1 transition ${
                activeTab === 'door' 
                  ? 'bg-rose-600 text-white shadow' 
                : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              <DoorOpen className="w-3.5 h-3.5" />
              Doors
            </button>

            <button
              onClick={() => setActiveTab('grill')}
              className={`flex-1 py-1.5 rounded-lg flex items-center justify-center gap-1 transition ${
                activeTab === 'grill' 
                  ? 'bg-rose-600 text-white shadow' 
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              <Shield className="w-3.5 h-3.5" />
              Grills
            </button>

            <button
              onClick={() => setActiveTab('pots')}
              className={`flex-1 py-1.5 rounded-lg flex items-center justify-center gap-1 transition ${
                activeTab === 'pots' 
                  ? 'bg-rose-600 text-white shadow' 
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60'
              }`}
            >
              <Flower2 className="w-3.5 h-3.5" />
              Pots ({pots.length})
            </button>
          </div>

          {/* Content Body */}
          <div className="flex-1 p-4 overflow-y-auto space-y-4 text-xs text-slate-300">
            {/* --------------------------------------------------- */}
            {/* TAB 1: WALL PAINT                                   */}
            {/* --------------------------------------------------- */}
            {activeTab === 'wall' && (
              <div className="space-y-4 animate-in fade-in">
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="font-semibold text-white">Wall Paint Color</span>
                    <span className="text-[10px] text-slate-400 font-mono">{wallColor.toUpperCase()}</span>
                  </div>
                  <p className="text-[11px] text-slate-400 mb-2">
                    Select a paint tone or enter custom hex. Works on all exterior and interior walls.
                  </p>

                  {/* Swatches Grid */}
                  <div className="grid grid-cols-4 gap-2 mb-3">
                    {WALL_PAINT_PRESETS.map((preset) => (
                      <button
                        key={preset.hex}
                        onClick={() => {
                          if (selectedElement && selectedElement.type === 'Wall') {
                            setCustomWallColors(prev => ({ ...prev, [selectedElement.id]: preset.hex }));
                          } else {
                            setWallColor(preset.hex);
                          }
                        }}
                        className={`group relative h-9 rounded-lg border transition-all flex items-center justify-center shadow-sm overflow-hidden ${
                          (selectedElement && selectedElement.type === 'Wall' 
                            ? customWallColors[selectedElement.id] === preset.hex 
                            : wallColor === preset.hex)
                            ? 'border-rose-400 ring-2 ring-rose-500/50 scale-105'
                            : 'border-slate-700 hover:border-slate-500 hover:scale-102'
                        }`}
                        style={{ backgroundColor: preset.hex }}
                        title={`${preset.name} (${preset.hex})`}
                      >
                        {(selectedElement && selectedElement.type === 'Wall' 
                          ? customWallColors[selectedElement.id] === preset.hex 
                          : wallColor === preset.hex) && (
                          <Check className={`w-4 h-4 ${['#f8fafc', '#fef3c7', '#fde68a', '#ddd6fe', '#fed7aa', '#86efac'].includes(preset.hex) ? 'text-slate-900' : 'text-white'}`} />
                        )}
                      </button>
                    ))}
                  </div>

                  {/* Custom Picker */}
                  <div className="flex items-center gap-2 bg-slate-950/60 p-2 rounded-xl border border-slate-800">
                    <input
                      type="color"
                      value={selectedElement && selectedElement.type === 'Wall' ? (customWallColors[selectedElement.id] || wallColor) : wallColor}
                      onChange={(e) => {
                        const val = e.target.value;
                        if (selectedElement && selectedElement.type === 'Wall') {
                          setCustomWallColors(prev => ({ ...prev, [selectedElement.id]: val }));
                        } else {
                          setWallColor(val);
                        }
                      }}
                      className="w-8 h-8 rounded-lg cursor-pointer border-0 bg-transparent"
                    />
                    <div className="flex-1 text-[11px]">
                      <span className="text-slate-400 block">Custom Paint Hex:</span>
                      <span className="font-mono text-white font-semibold">
                        {selectedElement && selectedElement.type === 'Wall' ? (customWallColors[selectedElement.id] || wallColor) : wallColor}
                      </span>
                    </div>
                    {selectedElement && selectedElement.type === 'Wall' && (
                      <button
                        onClick={() => {
                          setWallColor(customWallColors[selectedElement.id] || wallColor);
                        }}
                        className="px-2 py-1 bg-slate-800 hover:bg-slate-700 text-[10px] rounded-lg text-slate-200 border border-slate-700"
                        title="Set this color as default for all walls"
                      >
                        Set for All
                      </button>
                    )}
                  </div>
                </div>

                {/* Paint Sheen / Finish */}
                <div>
                  <span className="font-semibold text-white block mb-1.5">Paint Sheen & Finish</span>
                  <div className="grid grid-cols-3 gap-1.5">
                    {(['matte', 'satin', 'gloss'] as const).map((finish) => (
                      <button
                        key={finish}
                        onClick={() => setWallFinish(finish)}
                        className={`py-1.5 text-center rounded-lg text-[11px] font-semibold border transition capitalize ${
                          wallFinish === finish
                            ? 'bg-rose-600 border-rose-500 text-white'
                            : 'bg-slate-950 border-slate-800 text-slate-400 hover:text-white'
                        }`}
                      >
                        {finish}
                      </button>
                    ))}
                  </div>
                </div>

                {selectedElement && selectedElement.type === 'Wall' ? (
                  <div className="bg-rose-950/40 border border-rose-800/60 p-2.5 rounded-xl text-[11px] text-rose-300">
                    <p className="font-semibold flex items-center gap-1.5 mb-1">
                      <span className="w-2 h-2 rounded-full bg-rose-500" />
                      Individual Wall Selected:
                    </p>
                    <p className="text-slate-400">{selectedElement.room_name || 'Exterior'} ({selectedElement.area.toFixed(1)} m²)</p>
                    <button
                      onClick={() => {
                        const next = { ...customWallColors };
                        delete next[selectedElement.id];
                        setCustomWallColors(next);
                      }}
                      className="mt-2 text-[10px] text-rose-400 hover:text-rose-200 underline block"
                    >
                      Reset this wall to global color
                    </button>
                  </div>
                ) : (
                  <div className="bg-slate-950/50 p-2.5 rounded-xl border border-slate-800 text-[11px] text-slate-400">
                    💡 Click any wall directly in 3D to paint that specific wall individually!
                  </div>
                )}
              </div>
            )}

            {/* --------------------------------------------------- */}
            {/* TAB 2: DOOR COLORS                                  */}
            {/* --------------------------------------------------- */}
            {activeTab === 'door' && (
              <div className="space-y-4 animate-in fade-in">
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="font-semibold text-white">Door Wood Stains & Colors</span>
                    <span className="text-[10px] text-slate-400 font-mono">{doorColor.toUpperCase()}</span>
                  </div>
                  <p className="text-[11px] text-slate-400 mb-2">
                    Customize doors with real wood grains, rich walnut, modern charcoal, or classic white.
                  </p>

                  {/* Swatches Grid */}
                  <div className="grid grid-cols-3 gap-2 mb-3">
                    {DOOR_COLOR_PRESETS.map((preset) => (
                      <button
                        key={preset.hex}
                        onClick={() => {
                          if (selectedElement && selectedElement.type === 'Door') {
                            setCustomDoorColors(prev => ({ ...prev, [selectedElement.id]: preset.hex }));
                          } else {
                            setDoorColor(preset.hex);
                          }
                        }}
                        className={`group relative p-2 rounded-xl border transition-all flex flex-col items-center gap-1 text-center ${
                          (selectedElement && selectedElement.type === 'Door'
                            ? customDoorColors[selectedElement.id] === preset.hex
                            : doorColor === preset.hex)
                            ? 'border-rose-400 bg-slate-800 ring-2 ring-rose-500/50'
                            : 'border-slate-800 bg-slate-950/60 hover:border-slate-600'
                        }`}
                      >
                        <span 
                          className="w-7 h-7 rounded-lg shadow border border-black/30 block flex items-center justify-center"
                          style={{ backgroundColor: preset.hex }}
                        >
                          {(selectedElement && selectedElement.type === 'Door'
                            ? customDoorColors[selectedElement.id] === preset.hex
                            : doorColor === preset.hex) && (
                            <Check className={`w-3.5 h-3.5 ${preset.hex === '#f8fafc' ? 'text-slate-900' : 'text-white'}`} />
                          )}
                        </span>
                        <span className="text-[10px] font-medium text-slate-300 leading-tight">{preset.name}</span>
                      </button>
                    ))}
                  </div>

                  {/* Custom Picker */}
                  <div className="flex items-center gap-2 bg-slate-950/60 p-2 rounded-xl border border-slate-800">
                    <input
                      type="color"
                      value={selectedElement && selectedElement.type === 'Door' ? (customDoorColors[selectedElement.id] || doorColor) : doorColor}
                      onChange={(e) => {
                        const val = e.target.value;
                        if (selectedElement && selectedElement.type === 'Door') {
                          setCustomDoorColors(prev => ({ ...prev, [selectedElement.id]: val }));
                        } else {
                          setDoorColor(val);
                        }
                      }}
                      className="w-8 h-8 rounded-lg cursor-pointer border-0 bg-transparent"
                    />
                    <div className="flex-1 text-[11px]">
                      <span className="text-slate-400 block">Custom Door Color:</span>
                      <span className="font-mono text-white font-semibold">
                        {selectedElement && selectedElement.type === 'Door' ? (customDoorColors[selectedElement.id] || doorColor) : doorColor}
                      </span>
                    </div>
                  </div>
                </div>

                {selectedElement && selectedElement.type === 'Door' ? (
                  <div className="bg-rose-950/40 border border-rose-800/60 p-2.5 rounded-xl text-[11px] text-rose-300">
                    <p className="font-semibold flex items-center gap-1.5 mb-1">
                      <span className="w-2 h-2 rounded-full bg-rose-500" />
                      Selected Door: {selectedElement.room_name || 'Entrance'}
                    </p>
                    <button
                      onClick={() => {
                        const next = { ...customDoorColors };
                        delete next[selectedElement.id];
                        setCustomDoorColors(next);
                      }}
                      className="mt-1 text-[10px] text-rose-400 hover:text-rose-200 underline block"
                    >
                      Reset this door to global color
                    </button>
                  </div>
                ) : (
                  <div className="bg-slate-950/50 p-2.5 rounded-xl border border-slate-800 text-[11px] text-slate-400">
                    💡 Click any door in the 3D building to apply a unique wood finish!
                  </div>
                )}
              </div>
            )}

            {/* --------------------------------------------------- */}
            {/* TAB 3: GRILL COLOR & STYLE                          */}
            {/* --------------------------------------------------- */}
            {activeTab === 'grill' && (
              <div className="space-y-4 animate-in fade-in">
                {/* Toggle Grills */}
                <div className="flex items-center justify-between bg-slate-950/60 p-3 rounded-xl border border-slate-800">
                  <div>
                    <span className="font-semibold text-white block">Window Safety Grills</span>
                    <span className="text-[10px] text-slate-400">Architectural security & balcony grills</span>
                  </div>
                  <button
                    onClick={() => setShowGrills(!showGrills)}
                    className={`px-3 py-1.5 rounded-lg text-xs font-bold transition shadow ${
                      showGrills 
                        ? 'bg-rose-600 text-white' 
                        : 'bg-slate-800 text-slate-400 hover:text-white'
                    }`}
                  >
                    {showGrills ? 'ENABLED' : 'DISABLED'}
                  </button>
                </div>

                {showGrills && (
                  <>
                    {/* Grill Style */}
                    <div>
                      <span className="font-semibold text-white block mb-1.5">Grill Bar Pattern</span>
                      <div className="grid grid-cols-3 gap-1.5">
                        {[
                          { id: 'vertical', label: 'Vertical Bars' },
                          { id: 'grid', label: 'Security Grid' },
                          { id: 'modern', label: 'Modern Louver' }
                        ].map((s) => (
                          <button
                            key={s.id}
                            onClick={() => setGrillStyle(s.id as any)}
                            className={`py-1.5 px-2 text-center rounded-lg text-[10px] font-semibold border transition ${
                              grillStyle === s.id
                                ? 'bg-rose-600 border-rose-500 text-white shadow'
                                : 'bg-slate-950 border-slate-800 text-slate-400 hover:text-white'
                            }`}
                          >
                            {s.label}
                          </button>
                        ))}
                      </div>
                    </div>

                    {/* Grill Color Swatches */}
                    <div>
                      <div className="flex items-center justify-between mb-1.5">
                        <span className="font-semibold text-white">Grill Metallic Color</span>
                        <span className="text-[10px] text-slate-400 font-mono">{grillColor.toUpperCase()}</span>
                      </div>

                      <div className="grid grid-cols-3 gap-2 mb-3">
                        {GRILL_COLOR_PRESETS.map((preset) => (
                          <button
                            key={preset.hex}
                            onClick={() => setGrillColor(preset.hex)}
                            className={`p-2 rounded-xl border transition flex flex-col items-center gap-1 ${
                              grillColor === preset.hex
                                ? 'border-rose-400 bg-slate-800 ring-2 ring-rose-500/50'
                                : 'border-slate-800 bg-slate-950/60 hover:border-slate-600'
                            }`}
                          >
                            <span 
                              className="w-7 h-7 rounded-lg shadow border border-slate-700 flex items-center justify-center"
                              style={{ backgroundColor: preset.hex }}
                            >
                              {grillColor === preset.hex && (
                                <Check className={`w-3.5 h-3.5 ${preset.hex === '#ffffff' ? 'text-slate-900' : 'text-white'}`} />
                              )}
                            </span>
                            <span className="text-[10px] font-medium text-slate-300 leading-tight">{preset.name}</span>
                          </button>
                        ))}
                      </div>

                      {/* Custom Grill Color */}
                      <div className="flex items-center gap-2 bg-slate-950/60 p-2 rounded-xl border border-slate-800">
                        <input
                          type="color"
                          value={grillColor}
                          onChange={(e) => setGrillColor(e.target.value)}
                          className="w-8 h-8 rounded-lg cursor-pointer border-0 bg-transparent"
                        />
                        <div className="flex-1 text-[11px]">
                          <span className="text-slate-400 block">Custom Metallic Hex:</span>
                          <span className="font-mono text-white font-semibold">{grillColor}</span>
                        </div>
                      </div>
                    </div>
                  </>
                )}
              </div>
            )}

            {/* --------------------------------------------------- */}
            {/* TAB 4: POTS & DECOR                                 */}
            {/* --------------------------------------------------- */}
            {activeTab === 'pots' && (
              <div className="space-y-4 animate-in fade-in">
                {/* Add Pot Presets */}
                <div>
                  <span className="font-semibold text-white block mb-2">Add New Decorative Pots</span>
                  <div className="grid grid-cols-2 gap-2">
                    <button
                      onClick={() => handleAddPotPreset('terracotta')}
                      className="p-2.5 rounded-xl border border-slate-800 bg-slate-950/70 hover:bg-slate-800 hover:border-rose-500/50 text-left transition group"
                    >
                      <div className="flex items-center gap-2 mb-1">
                        <span className="w-5 h-5 rounded-full bg-amber-700/80 flex items-center justify-center text-xs">🏺</span>
                        <span className="font-semibold text-white group-hover:text-rose-400">Terracotta Pot</span>
                      </div>
                      <span className="text-[10px] text-slate-400 block">Classic clay pot with lush bush</span>
                    </button>

                    <button
                      onClick={() => handleAddPotPreset('ceramic')}
                      className="p-2.5 rounded-xl border border-slate-800 bg-slate-950/70 hover:bg-slate-800 hover:border-rose-500/50 text-left transition group"
                    >
                      <div className="flex items-center gap-2 mb-1">
                        <span className="w-5 h-5 rounded-full bg-slate-200 text-slate-900 flex items-center justify-center text-xs">🪴</span>
                        <span className="font-semibold text-white group-hover:text-rose-400">Ceramic Planter</span>
                      </div>
                      <span className="text-[10px] text-slate-400 block">Tall modern pot with snake plant</span>
                    </button>

                    <button
                      onClick={() => handleAddPotPreset('urn')}
                      className="p-2.5 rounded-xl border border-slate-800 bg-slate-950/70 hover:bg-slate-800 hover:border-rose-500/50 text-left transition group"
                    >
                      <div className="flex items-center gap-2 mb-1">
                        <span className="w-5 h-5 rounded-full bg-slate-700 flex items-center justify-center text-xs">🌴</span>
                        <span className="font-semibold text-white group-hover:text-rose-400">Garden Palm Urn</span>
                      </div>
                      <span className="text-[10px] text-slate-400 block">Stone urn with tropical palm</span>
                    </button>

                    <button
                      onClick={() => handleAddPotPreset('flower')}
                      className="p-2.5 rounded-xl border border-slate-800 bg-slate-950/70 hover:bg-slate-800 hover:border-rose-500/50 text-left transition group"
                    >
                      <div className="flex items-center gap-2 mb-1">
                        <span className="w-5 h-5 rounded-full bg-rose-700 flex items-center justify-center text-xs">🌸</span>
                        <span className="font-semibold text-white group-hover:text-rose-400">Flower Blossom</span>
                      </div>
                      <span className="text-[10px] text-slate-400 block">Decorative blossoming flowers</span>
                    </button>
                  </div>

                  {/* Twin Entrance Pots Button */}
                  <button
                    onClick={handleAddTwinPots}
                    className="w-full mt-2.5 py-2 px-3 rounded-xl bg-gradient-to-r from-amber-900/60 to-rose-950/60 hover:from-amber-900 hover:to-rose-900 border border-amber-700/50 text-amber-200 text-xs font-semibold flex items-center justify-center gap-2 transition shadow"
                  >
                    <span>👯 Place Twin Entrance Welcoming Pots</span>
                  </button>

                  {/* Click-to-place Mode */}
                  <button
                    onClick={() => setClickToPlacePot(!clickToPlacePot)}
                    className={`w-full mt-2 py-2 px-3 rounded-xl border text-xs font-semibold flex items-center justify-center gap-2 transition ${
                      clickToPlacePot 
                        ? 'bg-emerald-600 border-emerald-400 text-white animate-pulse'
                        : 'bg-slate-950 border-slate-800 text-slate-300 hover:bg-slate-800'
                    }`}
                  >
                    <Plus className="w-4 h-4" />
                    {clickToPlacePot ? 'Click 3D Scene to Place (ACTIVE)' : 'Click Scene to Position Pot'}
                  </button>
                </div>

                {/* Selected Pot Customization Card */}
                {selectedPot ? (
                  <div className="bg-slate-950 border border-rose-500/60 p-3 rounded-xl space-y-3">
                    <div className="flex items-center justify-between pb-2 border-b border-slate-800">
                      <div className="flex items-center gap-1.5">
                        <span className="w-2.5 h-2.5 rounded-full bg-rose-500 animate-pulse" />
                        <span className="font-bold text-white text-xs">{selectedPot.name}</span>
                      </div>
                      <button
                        onClick={() => handleDeletePot(selectedPot.id)}
                        className="text-rose-400 hover:text-rose-300 p-1"
                        title="Delete this Pot"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>

                    {/* Plant Type Selector */}
                    <div>
                      <span className="text-[10px] text-slate-400 block mb-1">Plant Foliage Type</span>
                      <div className="grid grid-cols-4 gap-1">
                        {(['foliage', 'snake', 'palm', 'flowers'] as const).map(pType => (
                          <button
                            key={pType}
                            onClick={() => handleUpdateSelectedPot('plantType', pType)}
                            className={`py-1 text-[10px] rounded-lg border capitalize font-semibold ${
                              selectedPot.plantType === pType
                                ? 'bg-rose-600 border-rose-500 text-white'
                                : 'bg-slate-900 border-slate-800 text-slate-400 hover:text-white'
                            }`}
                          >
                            {pType}
                          </button>
                        ))}
                      </div>
                    </div>

                    {/* Pot Color */}
                    <div>
                      <span className="text-[10px] text-slate-400 block mb-1">Pot Material Color</span>
                      <div className="flex items-center gap-1.5">
                        {POT_COLOR_PRESETS.map(pc => (
                          <button
                            key={pc.hex}
                            onClick={() => handleUpdateSelectedPot('potColor', pc.hex)}
                            className={`w-6 h-6 rounded-full border shadow transition ${
                              selectedPot.potColor === pc.hex ? 'ring-2 ring-rose-500 scale-110' : 'border-slate-700'
                            }`}
                            style={{ backgroundColor: pc.hex }}
                            title={pc.name}
                          />
                        ))}
                        <input
                          type="color"
                          value={selectedPot.potColor}
                          onChange={(e) => handleUpdateSelectedPot('potColor', e.target.value)}
                          className="w-6 h-6 rounded-full cursor-pointer border-0 bg-transparent"
                        />
                      </div>
                    </div>

                    {/* Scale Slider */}
                    <div>
                      <div className="flex justify-between text-[10px] text-slate-400 mb-1">
                        <span>Pot Size Scale</span>
                        <span>{selectedPot.scale.toFixed(2)}x</span>
                      </div>
                      <input
                        type="range"
                        min="0.5"
                        max="2.0"
                        step="0.05"
                        value={selectedPot.scale}
                        onChange={(e) => handleUpdateSelectedPot('scale', parseFloat(e.target.value))}
                        className="w-full accent-rose-500 cursor-pointer"
                      />
                    </div>

                    {/* Move Controls */}
                    <div>
                      <span className="text-[10px] text-slate-400 block mb-1">Nudge Position</span>
                      <div className="grid grid-cols-4 gap-1">
                        <button
                          onClick={() => handleMovePot('x', -0.2)}
                          className="py-1 bg-slate-900 hover:bg-slate-800 rounded border border-slate-800 text-[10px] text-slate-300"
                        >
                          ◀ West
                        </button>
                        <button
                          onClick={() => handleMovePot('x', 0.2)}
                          className="py-1 bg-slate-900 hover:bg-slate-800 rounded border border-slate-800 text-[10px] text-slate-300"
                        >
                          East ▶
                        </button>
                        <button
                          onClick={() => handleMovePot('z', -0.2)}
                          className="py-1 bg-slate-900 hover:bg-slate-800 rounded border border-slate-800 text-[10px] text-slate-300"
                        >
                          ▲ North
                        </button>
                        <button
                          onClick={() => handleMovePot('z', 0.2)}
                          className="py-1 bg-slate-900 hover:bg-slate-800 rounded border border-slate-800 text-[10px] text-slate-300"
                        >
                          South ▼
                        </button>
                      </div>
                    </div>
                  </div>
                ) : (
                  /* Placed Pots List */
                  <div>
                    <span className="font-semibold text-white block mb-1.5">Placed Pots in Scene ({pots.length})</span>
                    {pots.length === 0 ? (
                      <p className="text-[11px] text-slate-500 italic">No pots placed yet. Click above to add pots!</p>
                    ) : (
                      <div className="space-y-1.5 max-h-36 overflow-y-auto pr-1">
                        {pots.map((pot, idx) => (
                          <div
                            key={pot.id}
                            onClick={() => setSelectedPot(pot)}
                            className="flex items-center justify-between p-2 rounded-lg bg-slate-950/70 border border-slate-800 hover:border-slate-700 cursor-pointer text-[11px] transition"
                          >
                            <div className="flex items-center gap-2">
                              <span 
                                className="w-3.5 h-3.5 rounded-full border border-black/40"
                                style={{ backgroundColor: pot.potColor }}
                              />
                              <span className="text-slate-300 font-medium">{pot.name}</span>
                            </div>
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                handleDeletePot(pot.id);
                              }}
                              className="text-slate-500 hover:text-rose-400 p-1"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer Reset Action */}
          <div className="px-4 py-2.5 border-t border-slate-800 bg-slate-950/80 flex items-center justify-between">
            <button
              onClick={handleResetToDefaults}
              className="text-[11px] text-slate-400 hover:text-rose-400 transition flex items-center gap-1"
            >
              <RotateCcw className="w-3 h-3" />
              Reset All Defaults
            </button>
            <span className="text-[10px] text-emerald-400 flex items-center gap-1">
              <Check className="w-3 h-3" />
              Auto-Saved
            </span>
          </div>
        </div>
      )}

      {/* Selected Element BIM Metadata Inspector Card */}
      {selectedElement && !showCustomizer && (
        <div className="absolute bottom-4 left-4 max-w-sm w-full bg-slate-900/95 backdrop-blur-md border border-red-500/50 rounded-xl p-4 text-xs text-slate-300 shadow-2xl animate-fade-in z-20">
          <div className="flex items-center justify-between pb-2 mb-3 border-b border-slate-700">
            <div className="flex items-center gap-2">
              <span className="w-2.5 h-2.5 rounded-full bg-red-500 animate-pulse" />
              <span className="font-bold text-sm text-white">{selectedElement.type} Element</span>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => {
                  setShowCustomizer(true);
                  if (selectedElement.type === 'Wall') setActiveTab('wall');
                  if (selectedElement.type === 'Door') setActiveTab('door');
                  if (selectedElement.type === 'Window') setActiveTab('grill');
                }}
                className="px-2 py-0.5 rounded bg-rose-600/80 hover:bg-rose-600 text-white text-[10px] font-semibold"
              >
                Customize
              </button>
              <button
                onClick={() => setSelectedElement(null)}
                className="text-slate-400 hover:text-white"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
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

          {/* Quick Color Swatches directly on Inspector Card */}
          {selectedElement.type === 'Wall' && (
            <div className="mt-3 pt-2.5 border-t border-slate-800">
              <span className="text-[11px] text-slate-400 block mb-1.5 font-medium">Quick Wall Paint:</span>
              <div className="flex items-center gap-1.5">
                {WALL_PAINT_PRESETS.slice(0, 7).map(wp => (
                  <button
                    key={wp.hex}
                    onClick={() => {
                      setCustomWallColors(prev => ({ ...prev, [selectedElement.id]: wp.hex }));
                    }}
                    className="w-5 h-5 rounded-md border border-slate-700 transition hover:scale-110"
                    style={{ backgroundColor: wp.hex }}
                    title={wp.name}
                  />
                ))}
                <button
                  onClick={() => {
                    setShowCustomizer(true);
                    setActiveTab('wall');
                  }}
                  className="text-[10px] text-rose-400 hover:underline ml-1"
                >
                  More...
                </button>
              </div>
            </div>
          )}

          {selectedElement.type === 'Door' && (
            <div className="mt-3 pt-2.5 border-t border-slate-800">
              <span className="text-[11px] text-slate-400 block mb-1.5 font-medium">Quick Door Finish:</span>
              <div className="flex items-center gap-1.5">
                {DOOR_COLOR_PRESETS.slice(0, 6).map(dp => (
                  <button
                    key={dp.hex}
                    onClick={() => {
                      setCustomDoorColors(prev => ({ ...prev, [selectedElement.id]: dp.hex }));
                    }}
                    className="w-5 h-5 rounded-md border border-slate-700 transition hover:scale-110"
                    style={{ backgroundColor: dp.hex }}
                    title={dp.name}
                  />
                ))}
                <button
                  onClick={() => {
                    setShowCustomizer(true);
                    setActiveTab('door');
                  }}
                  className="text-[10px] text-rose-400 hover:underline ml-1"
                >
                  More...
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Bottom Hint */}
      <div className="absolute bottom-4 right-4 bg-slate-900/80 backdrop-blur-sm px-3 py-1.5 rounded-lg border border-slate-800 text-[11px] text-slate-400 pointer-events-none z-10 flex items-center gap-2">
        <span>Click element to customize • Left drag rotate • Right drag pan • Scroll zoom</span>
      </div>
    </div>
  );
}
