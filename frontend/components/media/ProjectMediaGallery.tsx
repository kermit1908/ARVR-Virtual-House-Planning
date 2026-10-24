'use client';

import React, { useState, useEffect } from 'react';
import { HOUSE_TEMPLATES, HouseTemplate } from '../../lib/houseTemplates';
import { BIMHouseConfig } from '../../types';
import {
  Image as ImageIcon,
  Video,
  Upload,
  ExternalLink,
  Plus,
  Trash2,
  Eye,
  CheckCircle2,
  Film,
  Sparkles,
  Maximize2,
  X
} from 'lucide-react';

interface ProjectMediaGalleryProps {
  projectId: string;
  onApplyTemplate: (config: BIMHouseConfig) => Promise<void>;
}

interface UploadedMediaItem {
  id: string;
  name: string;
  type: 'blueprint' | 'site_photo' | 'elevation' | 'video';
  url: string;
  date: string;
}

export default function ProjectMediaGallery({ projectId, onApplyTemplate }: ProjectMediaGalleryProps) {
  const [activeSubTab, setActiveSubTab] = useState<'templates' | 'blueprints' | 'videos'>('templates');
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const [applyingTemplateId, setApplyingTemplateId] = useState<string | null>(null);

  // Local storage uploaded media
  const [uploadedMedia, setUploadedMedia] = useState<UploadedMediaItem[]>([]);
  const [videoUrlInput, setVideoUrlInput] = useState('');
  const [videoTitleInput, setVideoTitleInput] = useState('');

  // Sample default videos
  const defaultVideos = [
    {
      id: 'demo-walkthrough',
      name: 'Architectural 3D BIM Walkthrough Simulation',
      url: 'https://www.youtube.com/embed/dQw4w9WgXcQ', // fallback or sample
      embed: true
    }
  ];

  useEffect(() => {
    try {
      const stored = localStorage.getItem(`smartbuild_media_${projectId}`);
      if (stored) {
        setUploadedMedia(JSON.parse(stored));
      }
    } catch {
      // ignore localstorage errors
    }
  }, [projectId]);

  const saveMedia = (items: UploadedMediaItem[]) => {
    setUploadedMedia(items);
    try {
      localStorage.setItem(`smartbuild_media_${projectId}`, JSON.stringify(items));
    } catch {
      // ignore
    }
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>, type: 'blueprint' | 'site_photo' | 'elevation') => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    const reader = new FileReader();
    reader.onload = () => {
      const newItem: UploadedMediaItem = {
        id: Date.now().toString(),
        name: file.name,
        type,
        url: reader.result as string,
        date: new Date().toLocaleDateString()
      };
      saveMedia([newItem, ...uploadedMedia]);
    };
    reader.readAsDataURL(file);
    e.target.value = '';
  };

  const handleAddVideo = () => {
    if (!videoUrlInput.trim()) return;
    const newItem: UploadedMediaItem = {
      id: Date.now().toString(),
      name: videoTitleInput.trim() || 'Site Walkthrough Video',
      type: 'video',
      url: videoUrlInput.trim(),
      date: new Date().toLocaleDateString()
    };
    saveMedia([newItem, ...uploadedMedia]);
    setVideoUrlInput('');
    setVideoTitleInput('');
  };

  const handleDeleteMedia = (id: string) => {
    saveMedia(uploadedMedia.filter((item) => item.id !== id));
  };

  const handleApply = async (template: HouseTemplate) => {
    try {
      setApplyingTemplateId(template.id);
      await onApplyTemplate(template.config);
    } finally {
      setApplyingTemplateId(null);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header & Sub-tab Bar */}
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-slate-800 pb-5">
          <div>
            <h2 className="text-xl font-bold text-white flex items-center gap-2">
              <ImageIcon className="w-5 h-5 text-red-500" />
              Project Media, Blueprints & Video Gallery
            </h2>
            <p className="text-xs text-slate-400 mt-1">
              Architectural reference renders, 2D blueprint overlays, site inspection photos, and walkthrough videos.
            </p>
          </div>

          <div className="flex items-center gap-2 bg-slate-950 p-1 rounded-xl border border-slate-800">
            <button
              onClick={() => setActiveSubTab('templates')}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 ${
                activeSubTab === 'templates' ? 'bg-red-600 text-white shadow-md shadow-red-600/25' : 'text-slate-400 hover:text-white'
              }`}
            >
              <Sparkles className="w-3.5 h-3.5" />
              Architectural Renders ({HOUSE_TEMPLATES.length})
            </button>
            <button
              onClick={() => setActiveSubTab('blueprints')}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 ${
                activeSubTab === 'blueprints' ? 'bg-red-600 text-white shadow-md shadow-red-600/25' : 'text-slate-400 hover:text-white'
              }`}
            >
              <Upload className="w-3.5 h-3.5" />
              Site & Blueprints ({uploadedMedia.filter((m) => m.type !== 'video').length})
            </button>
            <button
              onClick={() => setActiveSubTab('videos')}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 ${
                activeSubTab === 'videos' ? 'bg-red-600 text-white shadow-md shadow-red-600/25' : 'text-slate-400 hover:text-white'
              }`}
            >
              <Video className="w-3.5 h-3.5" />
              Videos & Walkthroughs ({uploadedMedia.filter((m) => m.type === 'video').length})
            </button>
          </div>
        </div>

        {/* SUBTAB 1: Architectural Templates Gallery */}
        {activeSubTab === 'templates' && (
          <div className="mt-6 space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {HOUSE_TEMPLATES.map((tmpl) => (
                <div
                  key={tmpl.id}
                  className="bg-slate-950/80 border border-slate-800 rounded-2xl overflow-hidden hover:border-red-500/50 transition flex flex-col group shadow-lg"
                >
                  {/* Image container */}
                  <div className="relative aspect-video w-full bg-slate-900 overflow-hidden cursor-pointer" onClick={() => setPreviewImage(tmpl.image)}>
                    <img
                      src={tmpl.image}
                      alt={tmpl.name}
                      className="w-full h-full object-cover group-hover:scale-105 transition duration-500"
                    />
                    <div className="absolute top-3 left-3 bg-slate-900/90 backdrop-blur-md px-2.5 py-1 rounded-lg text-[11px] font-bold text-red-400 border border-slate-700">
                      {tmpl.bhk} • {tmpl.totalAreaSqm} m²
                    </div>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        setPreviewImage(tmpl.image);
                      }}
                      className="absolute top-3 right-3 p-1.5 rounded-lg bg-slate-900/80 hover:bg-slate-800 text-slate-300 hover:text-white border border-slate-700 opacity-0 group-hover:opacity-100 transition"
                      title="Enlarge Image"
                    >
                      <Maximize2 className="w-3.5 h-3.5" />
                    </button>
                  </div>

                  {/* Details */}
                  <div className="p-4 flex-1 flex flex-col justify-between">
                    <div>
                      <h3 className="font-bold text-white text-sm tracking-tight">{tmpl.name}</h3>
                      <p className="text-[11px] text-red-400/90 font-medium mt-0.5">{tmpl.subtitle}</p>
                      <p className="text-xs text-slate-400 mt-2 line-clamp-3 leading-relaxed">
                        {tmpl.description}
                      </p>

                      <div className="mt-3 flex flex-wrap gap-1.5">
                        {tmpl.config.floors[0].rooms.slice(0, 4).map((r, i) => (
                          <span
                            key={i}
                            className="text-[10px] bg-slate-800/80 text-slate-300 px-2 py-0.5 rounded border border-slate-700/60"
                          >
                            {r.name} ({r.width}×{r.length}m)
                          </span>
                        ))}
                        {tmpl.config.floors[0].rooms.length > 4 && (
                          <span className="text-[10px] bg-slate-800/80 text-slate-400 px-2 py-0.5 rounded border border-slate-700/60">
                            +{tmpl.config.floors[0].rooms.length - 4} more
                          </span>
                        )}
                      </div>
                    </div>

                    <div className="mt-4 pt-3 border-t border-slate-800/80 flex items-center justify-between gap-2">
                      <button
                        onClick={() => setPreviewImage(tmpl.image)}
                        className="text-xs text-slate-400 hover:text-white flex items-center gap-1 font-semibold transition"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        Inspect Plan
                      </button>

                      <button
                        onClick={() => handleApply(tmpl)}
                        disabled={applyingTemplateId === tmpl.id}
                        className="bg-red-600 hover:bg-red-700 text-white text-xs font-semibold px-3 py-1.5 rounded-lg flex items-center gap-1.5 transition shadow-md shadow-red-600/20"
                      >
                        <CheckCircle2 className="w-3.5 h-3.5" />
                        {applyingTemplateId === tmpl.id ? 'Generating...' : 'Apply & Build 3D'}
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* SUBTAB 2: User Blueprints & Site Photos */}
        {activeSubTab === 'blueprints' && (
          <div className="mt-6 space-y-6">
            {/* Upload Zone */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <label className="border-2 border-dashed border-slate-800 hover:border-red-500/60 rounded-xl p-5 text-center cursor-pointer transition bg-slate-950/40 flex flex-col items-center justify-center">
                <input
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={(e) => handleFileUpload(e, 'blueprint')}
                />
                <Upload className="w-6 h-6 text-red-500 mb-2" />
                <span className="text-xs font-bold text-white">Upload 2D Blueprint / Plan</span>
                <span className="text-[11px] text-slate-400 mt-0.5">PNG, JPG, CAD export</span>
              </label>

              <label className="border-2 border-dashed border-slate-800 hover:border-emerald-500/60 rounded-xl p-5 text-center cursor-pointer transition bg-slate-950/40 flex flex-col items-center justify-center">
                <input
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={(e) => handleFileUpload(e, 'site_photo')}
                />
                <Upload className="w-6 h-6 text-emerald-400 mb-2" />
                <span className="text-xs font-bold text-white">Upload Site Survey Photo</span>
                <span className="text-[11px] text-slate-400 mt-0.5">Plot boundary, road access</span>
              </label>

              <label className="border-2 border-dashed border-slate-800 hover:border-amber-500/60 rounded-xl p-5 text-center cursor-pointer transition bg-slate-950/40 flex flex-col items-center justify-center">
                <input
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={(e) => handleFileUpload(e, 'elevation')}
                />
                <Upload className="w-6 h-6 text-amber-400 mb-2" />
                <span className="text-xs font-bold text-white">Upload Elevation / Finish</span>
                <span className="text-[11px] text-slate-400 mt-0.5">Material textures, facade</span>
              </label>
            </div>

            {/* Uploaded Gallery Grid */}
            {uploadedMedia.filter((m) => m.type !== 'video').length === 0 ? (
              <div className="bg-slate-950/40 border border-slate-800/80 rounded-xl p-10 text-center text-slate-400 text-xs">
                No custom blueprints or site photos uploaded yet. Click one of the upload buttons above to attach images to this project.
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {uploadedMedia
                  .filter((m) => m.type !== 'video')
                  .map((item) => (
                    <div
                      key={item.id}
                      className="bg-slate-950 border border-slate-800 rounded-xl overflow-hidden group relative"
                    >
                      <div
                        className="aspect-square bg-slate-900 cursor-pointer overflow-hidden"
                        onClick={() => setPreviewImage(item.url)}
                      >
                        <img src={item.url} alt={item.name} className="w-full h-full object-cover group-hover:scale-105 transition" />
                      </div>
                      <div className="p-2.5 flex items-center justify-between text-xs">
                        <div className="truncate">
                          <p className="font-semibold text-white truncate text-[11px]">{item.name}</p>
                          <span className="text-[10px] text-slate-400 capitalize">{item.type.replace('_', ' ')}</span>
                        </div>
                        <button
                          onClick={() => handleDeleteMedia(item.id)}
                          className="text-slate-500 hover:text-rose-400 p-1"
                          title="Delete image"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>
                  ))}
              </div>
            )}
          </div>
        )}

        {/* SUBTAB 3: Videos & Walkthroughs */}
        {activeSubTab === 'videos' && (
          <div className="mt-6 space-y-6">
            {/* Add Video Link Card */}
            <div className="bg-slate-950/60 border border-slate-800 rounded-xl p-4 flex flex-col md:flex-row gap-3 items-center">
              <input
                type="text"
                placeholder="Video Title (e.g. Drone Plot Survey or 3D Walkthrough)"
                value={videoTitleInput}
                onChange={(e) => setVideoTitleInput(e.target.value)}
                className="bg-slate-900 border border-slate-700 text-white text-xs px-3 py-2 rounded-lg w-full md:w-1/3"
              />
              <input
                type="url"
                placeholder="Video URL (MP4 link, YouTube/Vimeo embed, or cloud video)"
                value={videoUrlInput}
                onChange={(e) => setVideoUrlInput(e.target.value)}
                className="bg-slate-900 border border-slate-700 text-white text-xs px-3 py-2 rounded-lg w-full md:flex-1"
              />
              <button
                onClick={handleAddVideo}
                className="bg-red-600 hover:bg-red-700 text-white text-xs font-semibold px-4 py-2 rounded-lg flex items-center gap-1.5 transition whitespace-nowrap shadow-md shadow-red-600/20"
              >
                <Plus className="w-3.5 h-3.5" />
                Attach Video
              </button>
            </div>

            {/* Video List */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Built-in Walkthrough Video Recording Notice */}
              <div className="bg-slate-950 border border-slate-800 rounded-2xl p-5 flex flex-col justify-between">
                <div>
                  <div className="flex items-center gap-2 text-red-500 font-bold text-sm mb-2">
                    <Film className="w-4 h-4" />
                    Built-in 3D Walkthrough Video Recorder
                  </div>
                  <p className="text-xs text-slate-300 leading-relaxed">
                    You can record a live 60 FPS walkthrough video of your house directly from the{' '}
                    <span className="text-red-400 font-semibold">3D BIM Viewer</span> tab using the{' '}
                    <span className="text-rose-400 font-semibold">Record Video</span> button. The video captures your exact camera movements in WebM format without third-party screen recorders.
                  </p>
                </div>
                <div className="mt-4 pt-3 border-t border-slate-800 flex items-center justify-between text-xs text-slate-400">
                  <span>Browser Native WebGL Recorder</span>
                  <span className="text-emerald-400 font-mono">1080p / 60 FPS</span>
                </div>
              </div>

              {/* User Videos */}
              {uploadedMedia
                .filter((m) => m.type === 'video')
                .map((video) => (
                  <div key={video.id} className="bg-slate-950 border border-slate-800 rounded-2xl overflow-hidden">
                    <div className="aspect-video bg-black flex items-center justify-center">
                      {video.url.includes('youtube.com') || video.url.includes('youtu.be') ? (
                        <iframe
                          src={video.url.replace('watch?v=', 'embed/')}
                          title={video.name}
                          className="w-full h-full border-0"
                          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                          allowFullScreen
                        />
                      ) : (
                        <video src={video.url} controls className="w-full h-full object-contain" />
                      )}
                    </div>
                    <div className="p-3.5 flex items-center justify-between">
                      <div>
                        <h4 className="text-xs font-bold text-white">{video.name}</h4>
                        <span className="text-[10px] text-slate-400">{video.date}</span>
                      </div>
                      <button
                        onClick={() => handleDeleteMedia(video.id)}
                        className="text-slate-500 hover:text-rose-400 p-1"
                        title="Delete video"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                ))}
            </div>
          </div>
        )}
      </div>

      {/* Fullscreen Image Preview Modal */}
      {previewImage && (
        <div
          className="fixed inset-0 z-50 bg-black/90 backdrop-blur-md flex items-center justify-center p-4 cursor-pointer"
          onClick={() => setPreviewImage(null)}
        >
          <div className="relative max-w-5xl max-h-[90vh] bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden p-2">
            <button
              onClick={() => setPreviewImage(null)}
              className="absolute top-4 right-4 p-2 rounded-full bg-black/60 text-white hover:bg-black/90 transition z-10"
            >
              <X className="w-5 h-5" />
            </button>
            <img src={previewImage} alt="Full Render Preview" className="max-h-[85vh] w-auto mx-auto object-contain rounded-xl" />
          </div>
        </div>
      )}
    </div>
  );
}
