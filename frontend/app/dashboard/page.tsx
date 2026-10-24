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
  FolderOpen
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
      <div className="min-h-screen bg-slate-950 flex items-center justify-center text-slate-400 text-sm">
        Loading user workspace...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      {/* Top Navbar */}
      <header className="border-b border-slate-800 bg-slate-900/60 sticky top-0 z-40 backdrop-blur-md">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-red-600 flex items-center justify-center shadow-md shadow-red-600/30">
              <Home className="w-5 h-5 text-white" />
            </div>
            <span className="font-bold text-white tracking-tight">SMARTBUILD Workspace</span>
          </Link>

          <div className="flex items-center gap-4 text-xs">
            <span className="text-slate-400 hidden sm:inline">
              Signed in as <strong className="text-white">{user?.full_name}</strong>
            </span>
            <button
              onClick={handleLogout}
              className="px-3 py-1.5 rounded-lg border border-slate-700 bg-slate-800 hover:bg-slate-700 text-slate-300 flex items-center gap-1.5 transition"
            >
              <LogOut className="w-3.5 h-3.5" />
              Sign Out
            </button>
          </div>
        </div>
      </header>

      {/* Main Content Area */}
      <main className="max-w-7xl mx-auto px-6 py-10 w-full space-y-8 flex-1">
        {/* Workspace Greeting & Stats */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-slate-800">
          <div>
            <h1 className="text-2xl font-black text-white">Residential BIM Projects</h1>
            <p className="text-xs text-slate-400 mt-1">
              Select an active project or initialize a new residential floor plan to launch 3D, AR/VR, and 5D cost analysis.
            </p>
          </div>

          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-red-600 hover:bg-red-700 text-white font-semibold text-xs px-4 py-2.5 rounded-lg shadow-lg shadow-red-600/25 flex items-center gap-2 transition"
          >
            <Plus className="w-4 h-4" />
            Create New Project
          </button>
        </div>

        {/* Project Cards Grid */}
        {projects.length === 0 ? (
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-12 text-center space-y-4">
            <div className="w-12 h-12 rounded-full bg-slate-800 text-red-500 flex items-center justify-center mx-auto">
              <FolderOpen className="w-6 h-6" />
            </div>
            <h3 className="font-bold text-white text-base">No Residential Projects Yet</h3>
            <p className="text-xs text-slate-400 max-w-sm mx-auto">
              Start by creating your first residential project. Define your plot boundaries and let SMARTBUILD extrude the BIM building model.
            </p>
            <button
              onClick={() => setShowCreateModal(true)}
              className="bg-red-600 hover:bg-red-500 text-white font-semibold text-xs px-4 py-2 rounded-lg transition"
            >
              Create Project
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {projects.map((proj) => (
              <div
                key={proj.id}
                onClick={() => router.push(`/projects/${proj.id}`)}
                className="bg-slate-900 border border-slate-800 hover:border-red-500/50 rounded-2xl p-5 shadow-xl hover:shadow-red-500/5 transition cursor-pointer flex flex-col justify-between group"
              >
                <div>
                  <div className="flex items-start justify-between gap-2 mb-3">
                    <span className="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded bg-red-950/80 text-red-400 border border-red-800/60">
                      {proj.status.replace('_', ' ')}
                    </span>
                    <button
                      onClick={(e) => handleDeleteProject(proj.id, e)}
                      className="text-slate-600 hover:text-rose-400 transition p-1"
                      title="Delete Project"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>

                  <h3 className="text-base font-bold text-white group-hover:text-red-400 transition truncate">
                    {proj.name}
                  </h3>
                  <p className="text-xs text-slate-400 line-clamp-2 mt-1">
                    {proj.description || 'Custom parametric residential villa layout.'}
                  </p>
                </div>

                <div className="mt-6 pt-4 border-t border-slate-800/80 space-y-2 text-xs">
                  <div className="flex justify-between text-slate-400">
                    <span className="flex items-center gap-1.5">
                      <MapPin className="w-3.5 h-3.5 text-rose-400" />
                      {proj.location}
                    </span>
                    <span className="font-mono text-slate-300">
                      {proj.plot_area} m² Plot
                    </span>
                  </div>

                  <div className="flex justify-between items-center text-xs pt-1">
                    <span className="text-slate-500">Estimated Cost:</span>
                    <span className="font-bold text-white font-mono">
                      {formatLakhs(proj.estimated_cost)}
                    </span>
                  </div>

                  <div className="pt-2 flex items-center justify-end text-red-400 text-xs font-semibold group-hover:translate-x-1 transition-transform">
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
        <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-800 rounded-2xl max-w-md w-full p-6 shadow-2xl animate-fade-in">
            <div className="flex items-center justify-between pb-3 mb-4 border-b border-slate-800">
              <h3 className="font-bold text-white text-base">Create New Residential Project</h3>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-white">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateProject} className="space-y-4">
              <div>
                <label className="text-xs font-semibold text-slate-300 block mb-1">Project Name</label>
                <input
                  type="text"
                  required
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="e.g. Modern 3BHK Eco-Villa"
                  className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-500"
                />
              </div>

              <div>
                <label className="text-xs font-semibold text-slate-300 block mb-1">Regional Location (Rate Schedule)</label>
                <select
                  value={newLocation}
                  onChange={(e) => setNewLocation(e.target.value)}
                  className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-500"
                >
                  <option value="Bengaluru">Bengaluru (Karnataka PWD DSR)</option>
                  <option value="Chennai">Chennai (Tamil Nadu PWD)</option>
                  <option value="Hyderabad">Hyderabad (Telangana PWD)</option>
                  <option value="Delhi">Delhi NCR (CPWD DSR)</option>
                  <option value="Mumbai">Mumbai (Maharashtra PWD)</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-semibold text-slate-300 block mb-1">Plot Width (m)</label>
                  <input
                    type="number"
                    step="0.5"
                    min="5"
                    max="100"
                    value={newWidth}
                    onChange={(e) => setNewWidth(parseFloat(e.target.value) || 12)}
                    className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-500"
                  />
                </div>
                <div>
                  <label className="text-xs font-semibold text-slate-300 block mb-1">Plot Length (m)</label>
                  <input
                    type="number"
                    step="0.5"
                    min="5"
                    max="100"
                    value={newLength}
                    onChange={(e) => setNewLength(parseFloat(e.target.value) || 15)}
                    className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-500"
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-semibold text-slate-300 block mb-1">Project Description (Optional)</label>
                <textarea
                  value={newDesc}
                  onChange={(e) => setNewDesc(e.target.value)}
                  placeholder="e.g. Sustainable G+1 family house with terrace solar."
                  rows={2}
                  className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-500 resize-none"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 rounded-lg text-xs font-semibold text-slate-400 hover:text-white"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={creating}
                  className="bg-red-600 hover:bg-red-700 text-white px-5 py-2 rounded-lg text-xs font-semibold shadow-md shadow-red-600/25 transition"
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
