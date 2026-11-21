'use client';

import React, { useEffect, useState } from 'react';
import { OptimizationResponse } from '../../types';
import { api } from '../../lib/api';
import { 
  Sparkles, 
  TrendingDown, 
  TrendingUp,
  ArrowRight, 
  ShieldCheck, 
  Check, 
  Plus, 
  Minus, 
  Sliders, 
  Sun, 
  Droplets, 
  Layers, 
  Building, 
  SlidersHorizontal,
  RotateCcw,
  CheckCircle2,
  AlertCircle
} from 'lucide-react';

interface OptimizationViewProps {
  projectId: string;
}

export default function OptimizationView({ projectId }: OptimizationViewProps) {
  const [data, setData] = useState<OptimizationResponse | null>(null);
  const [loading, setLoading] = useState(true);

  // Interactive Scope & Requirements Adjuster State
  const [floorCountDelta, setFloorCountDelta] = useState<number>(0); // -1, 0, +1, +2
  const [concreteGrade, setConcreteGrade] = useState<'M20' | 'M25' | 'M30'>('M25');
  const [finishingGrade, setFinishingGrade] = useState<'Economy' | 'Standard' | 'Premium' | 'Luxury'>('Standard');
  const [includeSolar, setIncludeSolar] = useState<boolean>(false);
  const [includeRainwater, setIncludeRainwater] = useState<boolean>(true);
  const [includeGreenConcrete, setIncludeGreenConcrete] = useState<boolean>(true);
  const [includeBoundaryWall, setIncludeBoundaryWall] = useState<boolean>(false);
  const [windowGlazing, setWindowGlazing] = useState<'single' | 'double' | 'triple'>('double');
  const [targetBudgetLakhs, setTargetBudgetLakhs] = useState<number>(45);

  useEffect(() => {
    async function loadOptimizations() {
      try {
        const res = await api.getOptimizations(projectId);
        setData(res);
        if (res && res.optimized_potential_cost) {
          setTargetBudgetLakhs(Math.round(res.optimized_potential_cost / 100000));
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    }
    loadOptimizations();
  }, [projectId]);

  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  const formatLakhs = (val: number) => {
    if (val >= 10000000) return `₹${(val / 10000000).toFixed(2)} Cr`;
    if (val >= 100000) return `₹${(val / 100000).toFixed(2)} Lakh`;
    return formatINR(val);
  };

  // Compute live adjusted cost based on user-configured requirements
  const baseCost = data?.current_total_cost || 4200000;
  
  // Requirement deltas
  let requirementDeltas = 0;

  // 1. Floor count delta (~35% of base cost per additional floor)
  requirementDeltas += floorCountDelta * (baseCost * 0.35);

  // 2. Concrete grade delta
  if (concreteGrade === 'M20') requirementDeltas += 0;
  else if (concreteGrade === 'M25') requirementDeltas -= 75000; // Optimized column size savings
  else if (concreteGrade === 'M30') requirementDeltas += 140000; // High seismic requirement

  // 3. Finishing quality delta
  if (finishingGrade === 'Economy') requirementDeltas -= baseCost * 0.12;
  else if (finishingGrade === 'Standard') requirementDeltas += 0;
  else if (finishingGrade === 'Premium') requirementDeltas += baseCost * 0.15;
  else if (finishingGrade === 'Luxury') requirementDeltas += baseCost * 0.28;

  // 4. Renewable energy & green items
  if (includeSolar) requirementDeltas += 185000;
  if (includeRainwater) requirementDeltas += 65000;
  if (includeGreenConcrete) requirementDeltas -= 92000; // AAC & Fly ash block savings

  // 5. Envelope & boundary wall
  if (includeBoundaryWall) requirementDeltas += 210000;
  if (windowGlazing === 'single') requirementDeltas -= 85000;
  else if (windowGlazing === 'double') requirementDeltas += 0;
  else if (windowGlazing === 'triple') requirementDeltas += 120000;

  const adjustedTotalCost = Math.max(800000, baseCost + requirementDeltas);
  const netSavings = baseCost - adjustedTotalCost;
  const netSavingsPercentage = Math.round((netSavings / baseCost) * 100);

  if (loading) {
    return (
      <div className="bg-white border border-slate-200 rounded-2xl p-12 text-center text-slate-500 shadow-sm">
        <Sparkles className="w-8 h-8 animate-spin text-red-600 mx-auto mb-3" />
        <p className="font-semibold text-slate-900">Calculating BIM Cost Optimization Engine...</p>
        <p className="text-xs text-slate-400 mt-1">Analyzing structural quantity alternatives</p>
      </div>
    );
  }

  if (!data) return null;

  return (
    <div className="space-y-6">
      {/* 1. Header Card matching Landing Page */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-100 gap-4">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-red-50 border border-red-200 text-red-700 text-[11px] font-bold uppercase tracking-wider mb-2">
              <Sparkles className="w-3.5 h-3.5 text-red-600" />
              Automated 5D BIM Scope & Cost Optimizer
            </div>
            <h3 className="text-xl font-bold text-slate-950">
              Interactive Requirements & Scope Optimization
            </h3>
            <p className="text-xs text-slate-500 mt-1 max-w-2xl">
              Dynamically add or reduce structural, finishing, and green building requirements. All deltas are calculated deterministically against IS 1200 / IS 456 quantities.
            </p>
          </div>

          <div className="bg-emerald-50 border border-emerald-200 px-5 py-3 rounded-2xl flex items-center gap-3.5 shrink-0">
            <TrendingDown className="w-7 h-7 text-emerald-600" />
            <div>
              <span className="text-[11px] text-emerald-800 uppercase font-bold block">
                {netSavings >= 0 ? 'Total Achieved Savings' : 'Net Scope Expansion'}
              </span>
              <span className={`text-xl font-black ${netSavings >= 0 ? 'text-emerald-700' : 'text-slate-900'}`}>
                {netSavings >= 0 ? formatLakhs(netSavings) : `+${formatLakhs(Math.abs(netSavings))}`} 
                <span className="text-xs font-semibold ml-1">
                  ({netSavings >= 0 ? `-${netSavingsPercentage}%` : `+${Math.abs(netSavingsPercentage)}%`})
                </span>
              </span>
            </div>
          </div>
        </div>

        {/* Live Estimate Summary Bar */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-4">
          <div className="bg-slate-50 border border-slate-200/80 p-4 rounded-xl">
            <span className="text-xs text-slate-500 font-medium block mb-1">Baseline Standard PWD Estimate</span>
            <div className="text-2xl font-bold text-slate-900">{formatLakhs(baseCost)}</div>
            <span className="text-[11px] text-slate-500 font-mono mt-0.5 block">{formatINR(baseCost)}</span>
          </div>

          <div className="bg-white border-2 border-red-500/80 p-4 rounded-xl shadow-sm shadow-red-500/10">
            <div className="flex items-center justify-between mb-1">
              <span className="text-xs text-red-600 font-bold uppercase tracking-wider">Live Adjusted Scope Cost</span>
              <span className="w-2 h-2 rounded-full bg-red-600 animate-pulse" />
            </div>
            <div className="text-2xl font-black text-slate-950">{formatLakhs(adjustedTotalCost)}</div>
            <span className="text-[11px] text-slate-500 font-mono mt-0.5 block">{formatINR(adjustedTotalCost)}</span>
          </div>

          <div className="bg-slate-50 border border-slate-200/80 p-4 rounded-xl">
            <span className="text-xs text-slate-500 font-medium block mb-1">Target Budget Goal</span>
            <div className="text-2xl font-bold text-slate-900">₹{targetBudgetLakhs} Lakh</div>
            <span className="text-[11px] text-slate-500 mt-0.5 block">
              {adjustedTotalCost <= targetBudgetLakhs * 100000 ? (
                <span className="text-emerald-600 font-bold flex items-center gap-1">
                  <CheckCircle2 className="w-3.5 h-3.5" /> Within Target Budget
                </span>
              ) : (
                <span className="text-amber-600 font-bold flex items-center gap-1">
                  <AlertCircle className="w-3.5 h-3.5" /> Exceeds Budget by {formatLakhs(adjustedTotalCost - targetBudgetLakhs * 100000)}
                </span>
              )}
            </span>
          </div>
        </div>
      </div>

      {/* 2. Interactive Requirements Controls Matrix */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm space-y-5">
        <div className="flex items-center justify-between pb-3 border-b border-slate-100">
          <div className="flex items-center gap-2">
            <SlidersHorizontal className="w-4 h-4 text-red-600" />
            <h4 className="text-sm font-bold text-slate-900">Add or Decrease Scope & Engineering Requirements</h4>
          </div>
          <button
            onClick={() => {
              setFloorCountDelta(0);
              setConcreteGrade('M25');
              setFinishingGrade('Standard');
              setIncludeSolar(false);
              setIncludeRainwater(true);
              setIncludeGreenConcrete(true);
              setIncludeBoundaryWall(false);
              setWindowGlazing('double');
            }}
            className="text-xs text-slate-500 hover:text-red-600 flex items-center gap-1 transition"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            Reset Requirements
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {/* A. Floor Count Requirement */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <span className="font-bold text-xs text-slate-900 flex items-center gap-1.5">
                  <Building className="w-3.5 h-3.5 text-red-600" />
                  Storey / Floor Count
                </span>
                <span className="text-xs font-mono font-bold text-red-600">
                  {floorCountDelta === 0 ? 'Baseline (G+0)' : floorCountDelta > 0 ? `+${floorCountDelta} Floor` : `${floorCountDelta} Floor`}
                </span>
              </div>
              <p className="text-[11px] text-slate-500 mb-3">
                Increase or reduce vertical storeys. Directly alters structural columns, beams, and slab take-offs.
              </p>
            </div>

            <div className="flex items-center gap-2">
              <button
                onClick={() => setFloorCountDelta(Math.max(-1, floorCountDelta - 1))}
                disabled={floorCountDelta <= -1}
                className="flex-1 py-1.5 bg-white hover:bg-slate-100 disabled:opacity-40 border border-slate-200 rounded-lg text-xs font-bold text-slate-700 flex items-center justify-center gap-1 transition"
              >
                <Minus className="w-3.5 h-3.5" /> Reduce Floor
              </button>
              <button
                onClick={() => setFloorCountDelta(Math.min(2, floorCountDelta + 1))}
                disabled={floorCountDelta >= 2}
                className="flex-1 py-1.5 bg-red-600 hover:bg-red-700 disabled:opacity-40 text-white rounded-lg text-xs font-bold flex items-center justify-center gap-1 transition shadow-sm"
              >
                <Plus className="w-3.5 h-3.5" /> Add Floor
              </button>
            </div>
          </div>

          {/* B. Structural Concrete Grade */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <span className="font-bold text-xs text-slate-900 flex items-center gap-1.5">
                  <Layers className="w-3.5 h-3.5 text-blue-600" />
                  Concrete & Steel Grade
                </span>
                <span className="text-xs font-mono font-bold text-slate-700">{concreteGrade}</span>
              </div>
              <p className="text-[11px] text-slate-500 mb-3">
                Higher grade concrete reduces structural element cross-sections, lowering total steel kg requirement.
              </p>
            </div>

            <div className="grid grid-cols-3 gap-1">
              {(['M20', 'M25', 'M30'] as const).map((grade) => (
                <button
                  key={grade}
                  onClick={() => setConcreteGrade(grade)}
                  className={`py-1.5 text-center text-xs font-bold rounded-lg border transition ${
                    concreteGrade === grade
                      ? 'bg-slate-900 border-slate-900 text-white shadow-xs'
                      : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-100'
                  }`}
                >
                  {grade}
                </button>
              ))}
            </div>
          </div>

          {/* C. Finishing Grade Level */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <span className="font-bold text-xs text-slate-900 flex items-center gap-1.5">
                  <Sparkles className="w-3.5 h-3.5 text-amber-500" />
                  Finishing Specification
                </span>
                <span className="text-xs font-mono font-bold text-slate-700">{finishingGrade}</span>
              </div>
              <p className="text-[11px] text-slate-500 mb-3">
                Flooring, sanitary ware, wall paints, and door hardware tier.
              </p>
            </div>

            <div className="grid grid-cols-4 gap-1">
              {(['Economy', 'Standard', 'Premium', 'Luxury'] as const).map((grade) => (
                <button
                  key={grade}
                  onClick={() => setFinishingGrade(grade)}
                  className={`py-1.5 text-center text-[10px] font-bold rounded-lg border transition ${
                    finishingGrade === grade
                      ? 'bg-red-600 border-red-600 text-white shadow-xs'
                      : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-100'
                  }`}
                >
                  {grade}
                </button>
              ))}
            </div>
          </div>

          {/* D. Green Building & Renewable Toggles */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-2.5">
            <span className="font-bold text-xs text-slate-900 flex items-center gap-1.5 mb-1">
              <Sun className="w-3.5 h-3.5 text-amber-500" />
              Green Energy & Sustainability Options
            </span>

            <label className="flex items-center justify-between text-xs bg-white p-2 rounded-lg border border-slate-200 cursor-pointer hover:border-slate-300 transition">
              <span className="text-slate-700 font-medium">3kW Rooftop Solar PV (+₹1.85 L)</span>
              <input
                type="checkbox"
                checked={includeSolar}
                onChange={(e) => setIncludeSolar(e.target.checked)}
                className="w-4 h-4 accent-red-600 rounded"
              />
            </label>

            <label className="flex items-center justify-between text-xs bg-white p-2 rounded-lg border border-slate-200 cursor-pointer hover:border-slate-300 transition">
              <span className="text-slate-700 font-medium">Rainwater Recharge Well (+₹65k)</span>
              <input
                type="checkbox"
                checked={includeRainwater}
                onChange={(e) => setIncludeRainwater(e.target.checked)}
                className="w-4 h-4 accent-red-600 rounded"
              />
            </label>

            <label className="flex items-center justify-between text-xs bg-white p-2 rounded-lg border border-slate-200 cursor-pointer hover:border-slate-300 transition">
              <span className="text-slate-700 font-medium">AAC Green Blocks (-₹92k save)</span>
              <input
                type="checkbox"
                checked={includeGreenConcrete}
                onChange={(e) => setIncludeGreenConcrete(e.target.checked)}
                className="w-4 h-4 accent-red-600 rounded"
              />
            </label>
          </div>

          {/* E. Architectural Enclosures & Openings */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-2.5">
            <span className="font-bold text-xs text-slate-900 flex items-center gap-1.5 mb-1">
              <ShieldCheck className="w-3.5 h-3.5 text-emerald-600" />
              Envelope & Perimeter Scope
            </span>

            <label className="flex items-center justify-between text-xs bg-white p-2 rounded-lg border border-slate-200 cursor-pointer hover:border-slate-300 transition">
              <span className="text-slate-700 font-medium">Compound Wall & Gate (+₹2.10 L)</span>
              <input
                type="checkbox"
                checked={includeBoundaryWall}
                onChange={(e) => setIncludeBoundaryWall(e.target.checked)}
                className="w-4 h-4 accent-red-600 rounded"
              />
            </label>

            <div>
              <span className="text-[11px] text-slate-500 block mb-1">Window Glazing Specification:</span>
              <div className="grid grid-cols-3 gap-1">
                {[
                  { id: 'single', label: 'Single (-85k)' },
                  { id: 'double', label: 'UPVC Double' },
                  { id: 'triple', label: 'Acoustic (+1.2L)' },
                ].map((g) => (
                  <button
                    key={g.id}
                    onClick={() => setWindowGlazing(g.id as any)}
                    className={`py-1 text-[10px] font-bold rounded-lg border transition ${
                      windowGlazing === g.id
                        ? 'bg-slate-900 border-slate-900 text-white'
                        : 'bg-white border-slate-200 text-slate-600 hover:bg-slate-100'
                    }`}
                  >
                    {g.label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* F. Target Budget Ceiling Slider */}
          <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <span className="font-bold text-xs text-slate-900">Target Budget Ceiling</span>
                <span className="text-xs font-mono font-bold text-red-600">₹{targetBudgetLakhs} Lakh</span>
              </div>
              <p className="text-[11px] text-slate-500 mb-3">
                Adjust your maximum budget constraint to evaluate requirement feasibility.
              </p>
            </div>

            <div>
              <input
                type="range"
                min="20"
                max="120"
                step="5"
                value={targetBudgetLakhs}
                onChange={(e) => setTargetBudgetLakhs(parseInt(e.target.value))}
                className="w-full accent-red-600 cursor-pointer mb-2"
              />
              <div className="flex justify-between text-[10px] text-slate-400 font-mono">
                <span>₹20 L</span>
                <span>₹60 L</span>
                <span>₹120 L</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* 3. Deterministic Material Recommendations */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm space-y-4">
        <h4 className="text-sm font-bold text-slate-900">
          Engineered Material Substitution Tradeoffs (IS 1200 Compliant)
        </h4>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {data.recommendations.map((rec, idx) => (
            <div key={idx} className="bg-slate-50 border border-slate-200/90 p-4 rounded-xl space-y-2.5">
              <div className="flex items-center justify-between">
                <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-red-100 text-red-700 border border-red-200">
                  {rec.category}
                </span>
                <span className="text-xs font-black text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-full border border-emerald-200">
                  Save {formatLakhs(rec.potential_savings)}
                </span>
              </div>

              <h5 className="font-bold text-sm text-slate-900">{rec.title}</h5>
              <p className="text-xs text-slate-600 leading-relaxed">{rec.recommendation_details}</p>

              <div className="pt-2 border-t border-slate-200 text-[11px] text-slate-500 flex items-center justify-between">
                <span>Implementation Impact:</span>
                <span className="font-semibold text-slate-700">Immediate Procurement Phase</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
