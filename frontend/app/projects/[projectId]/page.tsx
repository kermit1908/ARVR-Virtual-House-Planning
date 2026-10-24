'use client';

import React, { useEffect, useState, use } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api } from '../../../lib/api';
import { 
  Project, 
  BIMModelResponse, 
  QTOResponse, 
  BOQResponse, 
  CostEstimateResponse, 
  AIPredictResponse, 
  BIMHouseConfig 
} from '../../../types';

import BIM3DViewer from '../../../components/viewer/BIM3DViewer';
import WebXRViewer from '../../../components/viewer/WebXRViewer';
import HouseConfigurator from '../../../components/house/HouseConfigurator';
import ProjectMediaGallery from '../../../components/media/ProjectMediaGallery';
import QuantitiesTable from '../../../components/cost/QuantitiesTable';
import CostEstimationView from '../../../components/cost/CostEstimationView';
import AIPredictionView from '../../../components/cost/AIPredictionView';
import OptimizationView from '../../../components/cost/OptimizationView';
import GanttScheduleView from '../../../components/schedule/GanttScheduleView';
import ProjectReportView from '../../../components/reports/ProjectReportView';

import {
  Building2,
  Home,
  Layers,
  Calculator,
  Cpu,
  Sparkles,
  Calendar,
  FileText,
  Glasses,
  Smartphone,
  ArrowLeft,
  MapPin,
  RefreshCw,
  Eye,
  Image as ImageIcon
} from 'lucide-react';

interface PageProps {
  params: Promise<{ projectId: string }>;
}

export default function ProjectWorkspacePage({ params }: PageProps) {
  const { projectId } = use(params);
  const router = useRouter();

  const [project, setProject] = useState<Project | null>(null);
  const [bimModel, setBimModel] = useState<BIMModelResponse | null>(null);
  const [qto, setQto] = useState<QTOResponse | null>(null);
  const [boq, setBoq] = useState<BOQResponse | null>(null);
  const [costEstimate, setCostEstimate] = useState<CostEstimateResponse | null>(null);
  const [aiPrediction, setAiPrediction] = useState<AIPredictResponse | null>(null);

  const [activeTab, setActiveTab] = useState<
    'viewer' | 'config' | 'media' | 'vr' | 'ar' | 'qto' | 'costs' | 'ai' | 'optimization' | 'schedule' | 'report'
  >('viewer');
  const [loading, setLoading] = useState(true);

  // Load project & BIM state
  const refreshProjectData = async () => {
    try {
      const proj = await api.getProject(projectId);
      setProject(proj);

      try {
        const bim = await api.getBIMModel(projectId);
        setBimModel(bim);

        const [qtoRes, boqRes, costRes] = await Promise.all([
          api.getQuantities(projectId),
          api.getBOQ(projectId),
          api.getCostEstimate(projectId)
        ]);

        setQto(qtoRes);
        setBoq(boqRes);
        setCostEstimate(costRes);
      } catch (bimErr) {
        // If BIM not generated yet, auto-switch to configurator
        setActiveTab('config');
      }
    } catch (err: any) {
      console.error(err);
      router.push('/dashboard');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshProjectData();
  }, [projectId]);

  // Handle BIM Generation from Configurator
  const handleGenerateBIM = async (config: BIMHouseConfig) => {
    const updatedBim = await api.generateHouse(projectId, config);
    setBimModel(updatedBim);
    await refreshProjectData();
    setActiveTab('viewer');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex items-center justify-center text-slate-400 text-sm">
        <RefreshCw className="w-5 h-5 animate-spin mr-2 text-red-500" />
        Loading BIM project workspace...
      </div>
    );
  }

  if (!project) return null;

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      {/* Top Header */}
      <header className="border-b border-slate-800 bg-slate-900/80 sticky top-0 z-40 backdrop-blur-md">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Link
              href="/dashboard"
              className="p-2 rounded-lg bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 transition"
              title="Return to Dashboard"
            >
              <ArrowLeft className="w-4 h-4" />
            </Link>

            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-base font-bold text-white tracking-tight">{project.name}</h1>
                <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-red-950/70 text-red-400 border border-red-800">
                  {project.status.replace('_', ' ')}
                </span>
              </div>
              <div className="flex items-center gap-2 text-xs text-slate-400">
                <span className="flex items-center gap-1">
                  <MapPin className="w-3 h-3 text-red-500" />
                  {project.location}
                </span>
                <span>•</span>
                <span>{project.plot_area} m² Plot</span>
                {bimModel && (
                  <>
                    <span>•</span>
                    <span className="text-red-400 font-semibold">{bimModel.builtup_area} m² Built-Up</span>
                  </>
                )}
              </div>
            </div>
          </div>

          <div className="flex items-center gap-2 text-xs">
            <button
              onClick={() => setActiveTab('report')}
              className="bg-red-600 hover:bg-red-700 text-white shadow-md shadow-red-600/20 px-3.5 py-1.5 rounded-lg font-semibold flex items-center gap-1.5 transition"
            >
              <FileText className="w-3.5 h-3.5" />
              Full Report
            </button>
          </div>
        </div>
      </header>

      {/* Main Workspace with Persistent Sidebar */}
      <div className="max-w-7xl mx-auto px-6 py-6 w-full flex-1 flex flex-col md:flex-row gap-6">
        {/* Navigation Sidebar */}
        <aside className="w-full md:w-64 shrink-0 space-y-1 bg-slate-900/60 border border-slate-800/80 p-3 rounded-2xl h-fit">
          <span className="text-[10px] uppercase tracking-wider font-bold text-slate-500 px-3 py-1.5 block">
            BIM Model & Visuals
          </span>

          <button
            onClick={() => setActiveTab('viewer')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'viewer' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Eye className="w-4 h-4" />
            3D BIM Viewer
          </button>

          <button
            onClick={() => setActiveTab('config')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'config' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Home className="w-4 h-4" />
            House Configurator
          </button>

          <button
            onClick={() => setActiveTab('media')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'media' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <ImageIcon className="w-4 h-4" />
            Media & Blueprints
          </button>

          <button
            onClick={() => setActiveTab('vr')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'vr' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Glasses className="w-4 h-4" />
            VR Walkthrough
          </button>

          <button
            onClick={() => setActiveTab('ar')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'ar' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Smartphone className="w-4 h-4" />
            AR Projection
          </button>

          <span className="text-[10px] uppercase tracking-wider font-bold text-slate-500 px-3 py-1.5 pt-4 block">
            5D Engineering & AI
          </span>

          <button
            onClick={() => setActiveTab('qto')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'qto' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Layers className="w-4 h-4" />
            Quantity Take-Off (QTO)
          </button>

          <button
            onClick={() => setActiveTab('costs')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'costs' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Calculator className="w-4 h-4" />
            5D Cost Estimation
          </button>

          <button
            onClick={() => setActiveTab('ai')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'ai' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Cpu className="w-4 h-4" />
            AI Cost Prediction
          </button>

          <button
            onClick={() => setActiveTab('optimization')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'optimization' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Sparkles className="w-4 h-4" />
            Cost Optimization
          </button>

          <button
            onClick={() => setActiveTab('schedule')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'schedule' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <Calendar className="w-4 h-4" />
            4D Construction Schedule
          </button>

          <button
            onClick={() => setActiveTab('report')}
            className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-semibold transition ${
              activeTab === 'report' ? 'bg-red-600 text-white shadow-md shadow-red-600/30' : 'text-slate-300 hover:bg-slate-800'
            }`}
          >
            <FileText className="w-4 h-4" />
            Project Report
          </button>
        </aside>

        {/* Dynamic Main Workspace View */}
        <main className="flex-1 min-w-0">
          {activeTab === 'viewer' && (
            bimModel ? (
              <BIM3DViewer
                elements={bimModel.elements}
                builtupArea={bimModel.builtup_area}
                floorsCount={bimModel.floors_count}
              />
            ) : (
              <div className="h-[620px] flex flex-col items-center justify-center bg-slate-900 border border-slate-800 rounded-xl p-8 text-center">
                <RefreshCw className="w-8 h-8 animate-spin text-red-500 mb-3" />
                <p className="text-white font-semibold">Generating 3D BIM Model...</p>
                <p className="text-xs text-slate-400 mt-1">Extracting structural spatial geometry</p>
              </div>
            )
          )}

          {activeTab === 'config' && (
            <HouseConfigurator
              initialConfig={bimModel?.raw_config}
              onGenerate={handleGenerateBIM}
              plotWidth={project.plot_width}
              plotLength={project.plot_length}
            />
          )}

          {activeTab === 'media' && (
            <ProjectMediaGallery
              projectId={projectId}
              onApplyTemplate={async (cfg) => {
                await handleGenerateBIM(cfg);
                setActiveTab('viewer');
              }}
            />
          )}

          {activeTab === 'vr' && (
            bimModel ? (
              <WebXRViewer
                mode="vr"
                elements={bimModel.elements}
                onBack={() => setActiveTab('viewer')}
              />
            ) : (
              <div className="h-[620px] flex flex-col items-center justify-center bg-slate-900 border border-slate-800 rounded-xl p-8 text-center">
                <RefreshCw className="w-8 h-8 animate-spin text-red-500 mb-3" />
                <p className="text-white font-semibold">Preparing VR Walkthrough Experience...</p>
              </div>
            )
          )}

          {activeTab === 'ar' && (
            bimModel ? (
              <WebXRViewer
                mode="ar"
                elements={bimModel.elements}
                onBack={() => setActiveTab('viewer')}
              />
            ) : (
              <div className="h-[620px] flex flex-col items-center justify-center bg-slate-900 border border-slate-800 rounded-xl p-8 text-center">
                <RefreshCw className="w-8 h-8 animate-spin text-red-500 mb-3" />
                <p className="text-white font-semibold">Initializing AR Projection...</p>
              </div>
            )
          )}

          {activeTab === 'qto' && (
            qto && boq ? (
              <QuantitiesTable qto={qto} boq={boq} />
            ) : (
              <div className="bg-slate-900 border border-slate-800 rounded-2xl p-12 text-center text-slate-400">
                <RefreshCw className="w-8 h-8 animate-spin text-red-500 mx-auto mb-3" />
                <p className="text-white font-semibold">Calculating IS 1200 Quantities & BOQ...</p>
              </div>
            )
          )}

          {activeTab === 'costs' && (
            costEstimate ? (
              <CostEstimationView costEstimate={costEstimate} />
            ) : (
              <div className="bg-slate-900 border border-slate-800 rounded-2xl p-12 text-center text-slate-400">
                <RefreshCw className="w-8 h-8 animate-spin text-red-500 mx-auto mb-3" />
                <p className="text-white font-semibold">Generating 5D Cost Estimate...</p>
              </div>
            )
          )}

          {activeTab === 'ai' && (
            <AIPredictionView projectId={projectId} />
          )}

          {activeTab === 'optimization' && (
            <OptimizationView projectId={projectId} />
          )}

          {activeTab === 'schedule' && (
            <GanttScheduleView projectId={projectId} />
          )}

          {activeTab === 'report' && (
            <ProjectReportView projectId={projectId} />
          )}
        </main>
      </div>
    </div>
  );
}
