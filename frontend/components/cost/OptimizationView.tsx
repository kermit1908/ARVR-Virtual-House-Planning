'use client';

import React, { useEffect, useState } from 'react';
import { OptimizationResponse } from '../../types';
import { api } from '../../lib/api';
import { Sparkles, TrendingDown, ArrowRight, ShieldCheck } from 'lucide-react';

interface OptimizationViewProps {
  projectId: string;
}

export default function OptimizationView({ projectId }: OptimizationViewProps) {
  const [data, setData] = useState<OptimizationResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadOptimizations() {
      try {
        const res = await api.getOptimizations(projectId);
        setData(res);
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

  if (loading) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-xl p-8 text-center text-slate-400">
        Calculating BIM material cost optimization scenarios...
      </div>
    );
  }

  if (!data) return null;

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-200 shadow-xl space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-800 gap-4">
        <div>
          <h3 className="text-lg font-bold text-white flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-amber-400" />
            BIM Material & Structural Cost Optimization Engine
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Deterministic cost-driver recommendations calculated from actual project dimensions and material take-offs. No fabricated savings.
          </p>
        </div>

        <div className="bg-emerald-950/60 border border-emerald-500/30 px-4 py-2 rounded-xl flex items-center gap-3">
          <TrendingDown className="w-6 h-6 text-emerald-400" />
          <div>
            <span className="text-[11px] text-emerald-300 uppercase font-semibold block">Total Potential Savings</span>
            <span className="text-xl font-black text-white">{formatLakhs(data.total_potential_savings)} ({data.savings_percentage}%)</span>
          </div>
        </div>
      </div>

      {/* Before / After Summary */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="bg-slate-950 border border-slate-800 p-4 rounded-xl">
          <span className="text-xs text-slate-400 block mb-1">Current Base Baseline Estimate</span>
          <div className="text-2xl font-bold text-slate-200">{formatLakhs(data.current_total_cost)}</div>
          <span className="text-xs text-slate-500 font-mono">{formatINR(data.current_total_cost)}</span>
        </div>

        <div className="bg-slate-950 border border-emerald-500/40 p-4 rounded-xl">
          <span className="text-xs text-emerald-400 font-semibold block mb-1">Optimized Potential Estimate</span>
          <div className="text-2xl font-black text-emerald-400">{formatLakhs(data.optimized_potential_cost)}</div>
          <span className="text-xs text-slate-400 font-mono">{formatINR(data.optimized_potential_cost)}</span>
        </div>
      </div>

      {/* Recommendations Cards */}
      <div className="space-y-4">
        <h4 className="text-sm font-semibold text-white">Calculated Material Alternative Scenarios</h4>
        <div className="grid grid-cols-1 gap-4">
          {data.recommendations.map((rec, idx) => (
            <div key={idx} className="bg-slate-950 border border-slate-800 p-4 rounded-xl space-y-3">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-red-950/60 text-red-400 border border-red-800/50">
                    {rec.category}
                  </span>
                  <h5 className="font-bold text-sm text-white">{rec.title}</h5>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-400 block">Calculated Savings:</span>
                  <span className="text-base font-black text-emerald-400">{formatLakhs(rec.potential_savings)}</span>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs bg-slate-900/60 p-3 rounded-lg border border-slate-800/80">
                <div>
                  <span className="text-slate-500 block mb-0.5">Current Specification:</span>
                  <span className="text-slate-300 font-medium">{rec.current_spec}</span>
                </div>
                <div>
                  <span className="text-emerald-400 font-semibold block mb-0.5 flex items-center gap-1">
                    <ArrowRight className="w-3.5 h-3.5" /> Proposed Alternative:
                  </span>
                  <span className="text-emerald-200 font-medium">{rec.proposed_spec}</span>
                </div>
              </div>

              <p className="text-xs text-slate-400 leading-relaxed">
                {rec.recommendation_details}
              </p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
