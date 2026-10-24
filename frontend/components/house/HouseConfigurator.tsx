'use client';

import React, { useState } from 'react';
import { BIMHouseConfig, FloorInput, RoomInput } from '../../types';
import { HOUSE_TEMPLATES, HouseTemplate } from '../../lib/houseTemplates';
import { Plus, Trash2, Home, Check, RefreshCw, Sparkles, Eye, X, CheckCircle2 } from 'lucide-react';

interface HouseConfiguratorProps {
  initialConfig?: BIMHouseConfig;
  onGenerate: (config: BIMHouseConfig) => Promise<void>;
  plotWidth: number;
  plotLength: number;
}

export default function HouseConfigurator({ initialConfig, onGenerate, plotWidth, plotLength }: HouseConfiguratorProps) {
  const [floorsCount, setFloorsCount] = useState<number>(initialConfig?.floors_count || 1);
  const [floorHeight, setFloorHeight] = useState<number>(initialConfig?.floor_height || 3.0);
  const [wallThickness, setWallThickness] = useState<number>(initialConfig?.wall_thickness || 0.23);
  const [quality, setQuality] = useState<string>(initialConfig?.quality || 'Standard');
  const [isGenerating, setIsGenerating] = useState<boolean>(false);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [previewImage, setPreviewImage] = useState<string | null>(null);

  const defaultRooms: RoomInput[] = [
    { name: 'Living Room', type: 'LivingRoom', width: 5.5, length: 4.5, floor_level: 0, floor_material: 'Tiles' },
    { name: 'Kitchen & Dining', type: 'Kitchen', width: 4.0, length: 3.5, floor_level: 0, floor_material: 'Tiles' },
    { name: 'Master Bedroom', type: 'Bedroom', width: 4.5, length: 4.0, floor_level: 0, floor_material: 'Tiles' },
    { name: 'Attached Bathroom', type: 'Bathroom', width: 2.2, length: 1.8, floor_level: 0, floor_material: 'Tiles' }
  ];

  const [floors, setFloors] = useState<FloorInput[]>(
    initialConfig?.floors || [
      { level: 0, height: 3.0, rooms: defaultRooms }
    ]
  );

  const handleApplyTemplate = (template: HouseTemplate) => {
    setSelectedTemplateId(template.id);
    setFloorsCount(template.config.floors_count);
    setFloorHeight(template.config.floor_height);
    setWallThickness(template.config.wall_thickness);
    setQuality(template.config.quality);
    setFloors(template.config.floors);
  };

  const handleFloorsChange = (newCount: number) => {
    setFloorsCount(newCount);
    const updatedFloors: FloorInput[] = [];

    for (let i = 0; i < newCount; i++) {
      if (floors[i]) {
        updatedFloors.push(floors[i]);
      } else {
        updatedFloors.push({
          level: i,
          height: floorHeight,
          rooms: [
            { name: `Bedroom ${i + 1}`, type: 'Bedroom', width: 4.5, length: 4.0, floor_level: i, floor_material: 'Tiles' },
            { name: `Study / Lounge`, type: 'Office', width: 4.0, length: 3.5, floor_level: i, floor_material: 'Tiles' }
          ]
        });
      }
    }
    setFloors(updatedFloors);
  };

  const handleAddRoom = (floorIndex: number) => {
    const updated = [...floors];
    updated[floorIndex].rooms.push({
      name: `New Room ${updated[floorIndex].rooms.length + 1}`,
      type: 'Bedroom',
      width: 4.0,
      length: 3.5,
      floor_level: floorIndex,
      floor_material: 'Tiles'
    });
    setFloors(updated);
  };

  const handleRemoveRoom = (floorIndex: number, roomIndex: number) => {
    const updated = [...floors];
    updated[floorIndex].rooms.splice(roomIndex, 1);
    setFloors(updated);
  };

  const handleRoomChange = (
    floorIndex: number,
    roomIndex: number,
    field: keyof RoomInput,
    value: string | number
  ) => {
    const updated = [...floors];
    updated[floorIndex].rooms[roomIndex] = {
      ...updated[floorIndex].rooms[roomIndex],
      [field]: value
    };
    setFloors(updated);
  };

  const totalBuiltup = floors.reduce((acc, f) => {
    return acc + f.rooms.reduce((rAcc, r) => rAcc + r.width * r.length, 0);
  }, 0);

  const plotArea = plotWidth * plotLength;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsGenerating(true);
    try {
      const config: BIMHouseConfig = {
        floors_count: floorsCount,
        floor_height: floorHeight,
        wall_thickness: wallThickness,
        quality,
        floors
      };
      await onGenerate(config);
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-200 shadow-xl space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between pb-4 border-b border-slate-800 gap-4">
        <div>
          <h3 className="text-lg font-bold text-white flex items-center gap-2">
            <Home className="w-5 h-5 text-red-500" />
            Parametric House Architecture Configuration
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Define structural parameters or pick from ready-made architectural templates below.
          </p>
        </div>

        <div className="flex items-center gap-3 bg-slate-800/80 px-4 py-2 rounded-lg border border-slate-700 text-xs">
          <div>
            <span className="text-slate-400 block">Total Built-Up:</span>
            <span className="font-bold text-red-500 text-sm">{totalBuiltup.toFixed(1)} m²</span>
          </div>
          <span className="text-slate-600">|</span>
          <div>
            <span className="text-slate-400 block">Plot Ground Area:</span>
            <span className="font-semibold text-slate-300">{plotArea.toFixed(1)} m²</span>
          </div>
        </div>
      </div>

      {/* Architectural Templates Carousel */}
      <div className="bg-slate-950/70 border border-slate-800/90 rounded-2xl p-5 space-y-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-amber-400" />
            <h4 className="text-xs font-bold text-white uppercase tracking-wider">
              Ready-to-Use Architectural Templates ({HOUSE_TEMPLATES.length} 3D Renders)
            </h4>
          </div>
          <span className="text-[11px] text-slate-400">Click a template to load its room layout</span>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3.5 pt-1">
          {HOUSE_TEMPLATES.map((tmpl) => {
            const isSelected = selectedTemplateId === tmpl.id;
            return (
              <div
                key={tmpl.id}
                onClick={() => handleApplyTemplate(tmpl)}
                className={`group relative rounded-xl border overflow-hidden cursor-pointer transition flex flex-col justify-between bg-slate-900 ${
                  isSelected
                    ? 'border-red-500 ring-2 ring-red-500/30 shadow-lg shadow-red-950/50'
                    : 'border-slate-800 hover:border-slate-700'
                }`}
              >
                {/* Thumbnail */}
                <div className="relative aspect-video w-full bg-slate-800 overflow-hidden">
                  <img
                    src={tmpl.image}
                    alt={tmpl.name}
                    className="w-full h-full object-cover group-hover:scale-105 transition duration-300"
                  />
                  <span className="absolute top-1.5 left-1.5 bg-black/75 px-1.5 py-0.5 rounded text-[10px] font-bold text-red-400 backdrop-blur-xs">
                    {tmpl.bhk}
                  </span>
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      setPreviewImage(tmpl.image);
                    }}
                    className="absolute top-1.5 right-1.5 p-1 rounded bg-black/60 hover:bg-black/90 text-slate-300 hover:text-white transition"
                    title="Enlarge Image"
                  >
                    <Eye className="w-3 h-3" />
                  </button>
                </div>

                {/* Info */}
                <div className="p-2.5 flex-1 flex flex-col justify-between">
                  <div>
                    <h5 className="font-bold text-xs text-white leading-tight">{tmpl.name}</h5>
                    <p className="text-[10px] text-slate-400 mt-1 line-clamp-2">{tmpl.subtitle}</p>
                  </div>

                  <div className="mt-2.5 pt-2 border-t border-slate-800/80 flex items-center justify-between text-[11px]">
                    <span className="text-slate-400 font-mono">{tmpl.totalAreaSqm} m²</span>
                    <span
                      className={`font-semibold flex items-center gap-1 ${
                        isSelected ? 'text-red-500' : 'text-slate-500 group-hover:text-slate-300'
                      }`}
                    >
                      {isSelected ? (
                        <>
                          <CheckCircle2 className="w-3 h-3 text-red-500" />
                          Applied
                        </>
                      ) : (
                        'Use Layout'
                      )}
                    </span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Global Parameters */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div>
          <label className="text-xs font-semibold text-slate-300 block mb-1.5">Number of Floors</label>
          <select
            value={floorsCount}
            onChange={(e) => handleFloorsChange(parseInt(e.target.value))}
            className="w-full bg-slate-800 border border-slate-700 text-white rounded-lg p-2.5 text-xs font-medium focus:ring-1 focus:ring-red-500 outline-none"
          >
            <option value={1}>1 Storey (Ground Floor G+0)</option>
            <option value={2}>2 Storeys (Duplex G+1)</option>
            <option value={3}>3 Storeys (Triplex G+2)</option>
          </select>
        </div>

        <div>
          <label className="text-xs font-semibold text-slate-300 block mb-1.5">Floor Height (m)</label>
          <input
            type="number"
            step="0.1"
            min="2.4"
            max="4.5"
            value={floorHeight}
            onChange={(e) => setFloorHeight(parseFloat(e.target.value) || 3.0)}
            className="w-full bg-slate-800 border border-slate-700 text-white rounded-lg p-2.5 text-xs font-medium focus:ring-1 focus:ring-red-500 outline-none"
          />
        </div>

        <div>
          <label className="text-xs font-semibold text-slate-300 block mb-1.5">Exterior Wall Thickness (m)</label>
          <select
            value={wallThickness}
            onChange={(e) => setWallThickness(parseFloat(e.target.value))}
            className="w-full bg-slate-800 border border-slate-700 text-white rounded-lg p-2.5 text-xs font-medium focus:ring-1 focus:ring-red-500 outline-none"
          >
            <option value={0.23}>0.23m (9-inch Standard Brick/AAC)</option>
            <option value={0.20}>0.20m (8-inch Concrete Block)</option>
            <option value={0.15}>0.15m (6-inch Partition/Light)</option>
          </select>
        </div>

        <div>
          <label className="text-xs font-semibold text-slate-300 block mb-1.5">Specification Standard</label>
          <select
            value={quality}
            onChange={(e) => setQuality(e.target.value)}
            className="w-full bg-slate-800 border border-slate-700 text-white rounded-lg p-2.5 text-xs font-medium focus:ring-1 focus:ring-red-500 outline-none"
          >
            <option value="Standard">Standard (IS Spec, Ceramic Tile)</option>
            <option value="Premium">Premium (Granite, Teak Doors)</option>
            <option value="Luxury">Luxury (Italian Marble, UPVC)</option>
          </select>
        </div>
      </div>

      {/* Floors and Rooms Editor */}
      <div className="space-y-6 pt-2">
        {floors.map((floor, fIdx) => (
          <div key={floor.level} className="bg-slate-950/60 border border-slate-800 rounded-xl p-4 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-800/80 pb-3">
              <span className="text-xs font-bold text-red-400 uppercase tracking-wider flex items-center gap-2">
                Floor Level {floor.level} {floor.level === 0 ? '(Ground Floor)' : `(Upper Level ${floor.level})`}
              </span>
              <button
                type="button"
                onClick={() => handleAddRoom(fIdx)}
                className="bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs px-3 py-1.5 rounded-lg flex items-center gap-1.5 transition font-semibold"
              >
                <Plus className="w-3.5 h-3.5 text-red-500" />
                Add Room
              </button>
            </div>

            {/* Room List Table / Cards */}
            <div className="space-y-3">
              {floor.rooms.map((room, rIdx) => (
                <div
                  key={rIdx}
                  className="grid grid-cols-1 md:grid-cols-12 gap-3 bg-slate-900/90 p-3 rounded-lg border border-slate-800/80 items-center text-xs"
                >
                  <div className="md:col-span-3">
                    <label className="text-[10px] text-slate-400 block mb-1">Room Name</label>
                    <input
                      type="text"
                      value={room.name}
                      onChange={(e) => handleRoomChange(fIdx, rIdx, 'name', e.target.value)}
                      className="w-full bg-slate-800 border border-slate-700 text-white rounded px-2.5 py-1.5 outline-none font-medium"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="text-[10px] text-slate-400 block mb-1">Room Type</label>
                    <select
                      value={room.type}
                      onChange={(e) => handleRoomChange(fIdx, rIdx, 'type', e.target.value)}
                      className="w-full bg-slate-800 border border-slate-700 text-white rounded px-2 py-1.5 outline-none"
                    >
                      <option value="LivingRoom">Living Room</option>
                      <option value="Bedroom">Bedroom</option>
                      <option value="Kitchen">Kitchen</option>
                      <option value="Bathroom">Bathroom</option>
                      <option value="Dining">Dining</option>
                      <option value="Office">Office / Study</option>
                    </select>
                  </div>

                  <div className="md:col-span-2">
                    <label className="text-[10px] text-slate-400 block mb-1">Width (m)</label>
                    <input
                      type="number"
                      step="0.1"
                      min="1.0"
                      value={room.width}
                      onChange={(e) => handleRoomChange(fIdx, rIdx, 'width', parseFloat(e.target.value) || 0)}
                      className="w-full bg-slate-800 border border-slate-700 text-white rounded px-2.5 py-1.5 outline-none font-mono"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="text-[10px] text-slate-400 block mb-1">Length (m)</label>
                    <input
                      type="number"
                      step="0.1"
                      min="1.0"
                      value={room.length}
                      onChange={(e) => handleRoomChange(fIdx, rIdx, 'length', parseFloat(e.target.value) || 0)}
                      className="w-full bg-slate-800 border border-slate-700 text-white rounded px-2.5 py-1.5 outline-none font-mono"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="text-[10px] text-slate-400 block mb-1">Flooring Finish</label>
                    <select
                      value={room.floor_material}
                      onChange={(e) => handleRoomChange(fIdx, rIdx, 'floor_material', e.target.value)}
                      className="w-full bg-slate-800 border border-slate-700 text-white rounded px-2 py-1.5 outline-none"
                    >
                      <option value="Tiles">Vitrified Tiles</option>
                      <option value="Marble">Italian Marble</option>
                      <option value="Granite">Polished Granite</option>
                      <option value="Hardwood">Hardwood Flooring</option>
                      <option value="Ceramic">Ceramic Tiles</option>
                    </select>
                  </div>

                  <div className="md:col-span-1 flex items-end justify-center pt-4 md:pt-0">
                    <button
                      type="button"
                      disabled={floor.rooms.length <= 1}
                      onClick={() => handleRemoveRoom(fIdx, rIdx)}
                      className="p-1.5 text-slate-500 hover:text-rose-400 disabled:opacity-30 disabled:pointer-events-none transition"
                      title="Delete Room"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>

      <div className="pt-2 flex justify-end">
        <button
          type="submit"
          disabled={isGenerating}
          className="bg-red-600 hover:bg-red-700 disabled:bg-slate-700 text-white font-semibold px-6 py-2.5 rounded-lg text-sm shadow-lg shadow-red-600/25 flex items-center gap-2 transition"
        >
          {isGenerating ? (
            <>
              <RefreshCw className="w-4 h-4 animate-spin" />
              Generating 3D BIM Model & Quantities...
            </>
          ) : (
            <>
              <Check className="w-4 h-4" />
              Generate 3D BIM Model
            </>
          )}
        </button>
      </div>

      {/* Image Preview Modal */}
      {previewImage && (
        <div
          className="fixed inset-0 z-50 bg-black/90 backdrop-blur-md flex items-center justify-center p-4 cursor-pointer"
          onClick={() => setPreviewImage(null)}
        >
          <div className="relative max-w-5xl max-h-[90vh] bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden p-2">
            <button
              type="button"
              onClick={() => setPreviewImage(null)}
              className="absolute top-4 right-4 p-2 rounded-full bg-black/60 text-white hover:bg-black/90 transition z-10"
            >
              <X className="w-5 h-5" />
            </button>
            <img src={previewImage} alt="Template Preview" className="max-h-[85vh] w-auto mx-auto object-contain rounded-xl" />
          </div>
        </div>
      )}
    </form>
  );
}
