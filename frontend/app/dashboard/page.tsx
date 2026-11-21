'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api } from '../../lib/api';
import { Project, User } from '../../types';
import { 
  Home,
  Plus, 
  MapPin, 
  Calendar, 
  ArrowRight, 
  LogOut, 
  Trash2, 
  Layers, 
  X,
  FolderOpen,
  Sparkles,
  Building2,
  CheckCircle2
} from 'lucide-react';

export default function DashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<User | null>(null);
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);

  // New Project Form State
  const [newName, setNewName] = useState('');
  const [newLocation, setNewLocation] = useState('Bengaluru');
  const [newWidth, setNewWidth] = useState(12.0);
  const [newLength, setNewLength] = useState(15.0);
  const [newDesc, setNewDesc] = useState('');
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    async function initDashboard() {
      try {
        const currentUser = await api.getMe();
        setUser(currentUser);
        const userProjects = await api.listProjects();
        setProjects(userProjects);
      } catch (err) {
        // If unauthenticated, redirect to login
        router.push('/login');
      } finally {
        setLoading(false);
      }
    }
    initDashboard();
  }, [router]);

  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreating(true);
    try {
      const created = await api.createProject(newName, newLocation, newWidth, newLength, newDesc);
      setProjects([created, ...projects]);
      setShowCreateModal(false);
      setNewName('');
      setNewDesc('');
      router.push(`/projects/${created.id}`);
    } catch (err: any) {
      alert(err.message || 'Failed to create project');
    } finally {
      setCreating(false);
    }
  };

  const handleDeleteProject = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm('Are you sure you want to permanently delete this project?')) return;
    try {
      await api.deleteProject(id);
      setProjects(projects.filter((p) => p.id !== id));
    } catch (err: any) {
      alert(err.message || 'Failed to delete project');
    }
  };

  const handleLogout = () => {
    api.logout();
    router.push('/login');
  };

  const formatLakhs = (val: number | null) => {
    if (!val) return 'Pending Generation';
    if (val >= 10000000) return `₹${(val / 10000000).toFixed(2)} Cr`;
    if (val >= 100000) return `₹${(val / 100000).toFixed(2)} Lakh`;
    return `₹${val.toLocaleString()}`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center text-slate-500 text-sm font-sans">
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 border-2 border-red-600 border-t-transparent rounded-full animate-spin" />
          <span>Loading user workspace...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 bg-architectural-grid text-slate-900 flex flex-col font-sans">
      {/* Top Navbar matching Landing Page */}
      <header className="border-b border-slate-200 bg-white/95 sticky top-0 z-40 backdrop-blur-md">
        <div className="max-w-7xl mx-auto px-6 h-18 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-3 group">
            <div className="w-10 h-10 rounded-xl bg-red-600 flex items-center justify-center shadow-md shadow-red-600/25 group-hover:scale-105 transition">
              <Home className="w-5 h-5 text-white" />
            </div>
            <span className="font-extrabold text-xl text-slate-950 tracking-tight">SMARTBUILD Workspace</span>
          </Link>

          <div className="flex items-center gap-4 text-xs">
            <span className="text-slate-600 hidden sm:inline">
              Signed in as <strong className="text-slate-900 font-bold">{user?.full_name}</strong>
            </span>
            <button
              onClick={handleLogout}
              className="px-3.5 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 font-semibold flex items-center gap-1.5 transition shadow-xs"
            >
              <LogOut className="w-3.5 h-3.5 text-slate-500" />
              Sign Out
            </button>
          </div>
        </div>
      </header>

      {/* Main Content Area */}
      <main className="max-w-7xl mx-auto px-6 py-10 w-full space-y-8 flex-1">
        {/* Workspace Greeting & Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-slate-200">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-red-50 border border-red-200 text-red-700 text-[11px] font-bold uppercase tracking-wider mb-2">
              <Building2 className="w-3.5 h-3.5 text-red-600" />
              Active Project Portfolio
            </div>
            <h1 className="text-3xl font-black text-slate-950 tracking-tight">Residential BIM Projects</h1>
            <p className="text-xs text-slate-500 mt-1 max-w-xl">
              Select an active project or initialize a new residential floor plan to launch 3D BIM visualization, AR/VR walkthrough, and 5D cost analysis.
            </p>
          </div>

          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-red-600 hover:bg-red-700 text-white font-bold text-xs px-5 py-3 rounded-xl shadow-md shadow-red-600/25 flex items-center gap-2 transition shrink-0"
          >
            <Plus className="w-4 h-4" />
            Create New Project
          </button>
        </div>

        {/* Project Cards Grid */}
        {projects.length === 0 ? (
          <div className="bg-white border border-slate-200/90 rounded-2xl p-12 text-center space-y-4 shadow-sm">
            <div className="w-14 h-14 rounded-2xl bg-red-50 border border-red-200 text-red-600 flex items-center justify-center mx-auto shadow-xs">
              <FolderOpen className="w-7 h-7" />
            </div>
            <h3 className="font-bold text-slate-950 text-base">No Residential Projects Yet</h3>
            <p className="text-xs text-slate-500 max-w-sm mx-auto leading-relaxed">
              Start by creating your first residential project. Define your plot boundaries and let SMARTBUILD extrude the BIM building model.
            </p>
            <button
              onClick={() => setShowCreateModal(true)}
              className="bg-red-600 hover:bg-red-700 text-white font-bold text-xs px-5 py-2.5 rounded-xl shadow-md shadow-red-600/25 transition inline-flex items-center gap-2"
            >
              <Plus className="w-4 h-4" />
              Create Project
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {projects.map((proj) => (
              <div
                key={proj.id}
                onClick={() => router.push(`/projects/${proj.id}`)}
                className="bg-white border border-slate-200/90 hover:border-red-500/80 rounded-2xl p-6 shadow-sm hover:shadow-xl hover:shadow-red-600/10 transition duration-200 cursor-pointer flex flex-col justify-between group"
              >
                <div>
                  <div className="flex items-start justify-between gap-2 mb-3">
                    <span className="text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full bg-red-50 text-red-700 border border-red-200">
                      {proj.status.replace('_', ' ')}
                    </span>
                    <button
                      onClick={(e) => handleDeleteProject(proj.id, e)}
                      className="text-slate-400 hover:text-red-600 transition p-1"
                      title="Delete Project"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>

                  <h3 className="text-base font-bold text-slate-900 group-hover:text-red-600 transition truncate">
                    {proj.name}
                  </h3>
                  <p className="text-xs text-slate-500 line-clamp-2 mt-1 leading-relaxed">
                    {proj.description || 'Custom parametric residential villa layout.'}
                  </p>
                </div>

                <div className="mt-6 pt-4 border-t border-slate-100 space-y-2 text-xs">
                  <div className="flex justify-between text-slate-500">
                    <span className="flex items-center gap-1.5 font-medium">
                      <MapPin className="w-3.5 h-3.5 text-red-600" />
                      {proj.location}
                    </span>
                    <span className="font-mono text-slate-700 font-semibold">
                      {proj.plot_area} m² Plot
                    </span>
                  </div>

                  <div className="flex justify-between items-center text-xs pt-1">
                    <span className="text-slate-500">Estimated Cost:</span>
                    <span className="font-bold text-slate-900 font-mono text-sm">
                      {formatLakhs(proj.estimated_cost)}
                    </span>
                  </div>

                  <div className="pt-2 flex items-center justify-end text-red-600 text-xs font-bold group-hover:translate-x-1 transition-transform">
                    Open Project Workspace <ArrowRight className="w-3.5 h-3.5 ml-1" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Create Project Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl animate-fade-in">
            <div className="flex items-center justify-between pb-3 mb-4 border-b border-slate-100">
              <h3 className="font-bold text-slate-950 text-base">Create New Residential Project</h3>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-slate-700">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateProject} className="space-y-4">
              <div>
                <label className="text-xs font-bold text-slate-700 block mb-1.5">Project Name</label>
                <input
                  type="text"
                  required
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="e.g. Modern 3BHK Eco-Villa"
                  className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
                />
              </div>

              <div>
                <label className="text-xs font-bold text-slate-700 block mb-1.5">Regional Location (Rate Schedule)</label>
                <select
                  value={newLocation}
                  onChange={(e) => setNewLocation(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
                >
                  <option value="Bengaluru">Bengaluru (Karnataka PWD DSR)</option>
                  <option value="Chennai">Chennai (Tamil Nadu PWD)</option>
                  <option value="Hyderabad">Hyderabad (Telangana PWD)</option>
                  <option value="Delhi">Delhi NCR (CPWD DSR)</option>
                  <option value="Mumbai">Mumbai (Maharashtra PWD)</option>
                  <option value="Pune">Pune (Maharashtra PWD)</option>
                  <option value="Kolkata">Kolkata (West Bengal PWD)</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-bold text-slate-700 block mb-1.5">Plot Width (m)</label>
                  <input
                    type="number"
                    step="0.5"
                    min="5"
                    max="100"
                    value={newWidth}
                    onChange={(e) => setNewWidth(parseFloat(e.target.value) || 12)}
                    className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
                  />
                </div>
                <div>
                  <label className="text-xs font-bold text-slate-700 block mb-1.5">Plot Length (m)</label>
                  <input
                    type="number"
                    step="0.5"
                    min="5"
                    max="100"
                    value={newLength}
                    onChange={(e) => setNewLength(parseFloat(e.target.value) || 15)}
                    className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-bold text-slate-700 block mb-1.5">Project Description (Optional)</label>
                <textarea
                  value={newDesc}
                  onChange={(e) => setNewDesc(e.target.value)}
                  placeholder="e.g. Sustainable G+1 family house with terrace solar."
                  rows={2}
                  className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 resize-none transition"
                />
              </div>

              <div className="flex justify-end gap-2.5 pt-2 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 rounded-xl text-xs font-semibold text-slate-600 hover:text-slate-900 hover:bg-slate-100"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={creating}
                  className="bg-red-600 hover:bg-red-700 text-white px-5 py-2.5 rounded-xl text-xs font-bold shadow-md shadow-red-600/25 transition"
                >
                  {creating ? 'Creating...' : 'Initialize Project'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
