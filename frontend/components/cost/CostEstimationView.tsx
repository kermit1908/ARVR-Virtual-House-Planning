'use client';

import React from 'react';
import { CostEstimateResponse } from '../../types';
import { Calculator, MapPin, ShieldAlert, Award } from 'lucide-react';

interface CostEstimationViewProps {
  costEstimate: CostEstimateResponse;
}

export default function CostEstimationView({ costEstimate }: CostEstimationViewProps) {
  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  const formatLakhs = (val: number) => {
    if (val >= 10000000) return `₹${(val / 10000000).toFixed(2)} Cr`;
    if (val >= 100000) return `₹${(val / 100000).toFixed(2)} Lakh`;
    return formatINR(val);
  };

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-200 shadow-xl space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-800 gap-4">
        <div>
          <h3 className="text-lg font-bold text-white flex items-center gap-2">
            <Calculator className="w-5 h-5 text-red-500" />
            5D BIM Construction Cost Model (Rule-Based Engineering Estimate)
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Dynamic cost calculations connecting structural BIM elements with location-specific market reference rates.
          </p>
        </div>

        <div className="flex items-center gap-2 text-xs">
          <span className="bg-slate-800 border border-slate-700 px-3 py-1.5 rounded-lg flex items-center gap-1.5 text-slate-300">
            <MapPin className="w-3.5 h-3.5 text-rose-400" />
            {costEstimate.location} Reference Rates
          </span>
          <span className="bg-slate-800 border border-slate-700 px-3 py-1.5 rounded-lg flex items-center gap-1.5 text-slate-300">
            <Award className="w-3.5 h-3.5 text-amber-400" />
            {costEstimate.quality} Grade
          </span>
        </div>
      </div>

      {/* Main Highlights Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-slate-950 border border-slate-800 rounded-xl p-5 shadow-lg">
          <span className="text-xs text-slate-400 block mb-1">Total Preliminary Estimate</span>
          <div className="text-2xl font-black text-white tracking-tight">
            {formatLakhs(costEstimate.total_rule_based_cost)}
          </div>
          <span className="text-xs font-mono text-slate-400 mt-1 block">
            {formatINR(costEstimate.total_rule_based_cost)}
          </span>
        </div>

        <div className="bg-slate-950 border border-slate-800 rounded-xl p-5 shadow-lg">
          <span className="text-xs text-slate-400 block mb-1">Cost per Unit Built-Up Area</span>
          <div className="text-2xl font-black text-red-500 tracking-tight">
            ₹{costEstimate.cost_per_sqm.toLocaleString()} <span className="text-xs font-normal text-slate-400">/ m²</span>
          </div>
          <span className="text-xs text-slate-400 mt-1 block">
            ≈ ₹{Math.round(costEstimate.cost_per_sqm / 10.764).toLocaleString()} / sq.ft
          </span>
        </div>

        <div className="bg-slate-950 border border-slate-800 rounded-xl p-5 shadow-lg">
          <span className="text-xs text-slate-400 block mb-1">Total Built-Up Footprint</span>
          <div className="text-2xl font-black text-emerald-400 tracking-tight">
            {costEstimate.builtup_area} <span className="text-xs font-normal text-slate-400">m²</span>
          </div>
          <span className="text-xs text-slate-400 mt-1 block">
            ≈ {(costEstimate.builtup_area * 10.764).toFixed(0)} sq.ft residential area
          </span>
        </div>
      </div>

      {/* Detailed Category Breakdown */}
      <div>
        <h4 className="text-sm font-semibold text-slate-200 mb-3">Cost Composition Breakdown</h4>
        <div className="space-y-3">
          {costEstimate.breakdown.map((item, idx) => (
            <div key={idx} className="bg-slate-950/70 border border-slate-800 p-3.5 rounded-lg">
              <div className="flex items-center justify-between text-xs mb-1.5">
                <span className="font-semibold text-slate-300">{item.category}</span>
                <div className="flex items-center gap-3">
                  <span className="font-mono text-slate-400">{item.percentage}%</span>
                  <span className="font-mono font-bold text-white">{formatINR(item.amount)}</span>
                </div>
              </div>
              <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                <div
                  className="bg-red-600 h-2 rounded-full transition-all duration-500"
                  style={{ width: `${Math.min(100, item.percentage)}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Disclaimer */}
      <div className="bg-amber-950/30 border border-amber-800/40 p-3.5 rounded-lg text-xs text-amber-200/90 flex items-start gap-2">
        <ShieldAlert className="w-4 h-4 text-amber-400 shrink-0 mt-0.5" />
        <p className="leading-relaxed">
          <strong>Academic Planning Disclaimer:</strong> {costEstimate.disclaimer}
        </p>
      </div>
    </div>
  );
}
