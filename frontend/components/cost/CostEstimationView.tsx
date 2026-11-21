'use client';

import React, { useState } from 'react';
import { CostEstimateResponse } from '../../types';
import { Calculator, MapPin, Award, ShieldAlert, Sliders, FileSpreadsheet, ArrowRight, Percent } from 'lucide-react';

interface CostEstimationViewProps {
  costEstimate: CostEstimateResponse;
}

// Regional PWD DSR Schedule Multipliers relative to Bengaluru standard baseline
const CITY_RATES: Record<string, { multiplier: number; ratePerSqft: number; state: string }> = {
  'Bengaluru': { multiplier: 1.0, ratePerSqft: 1825, state: 'Karnataka' },
  'Chennai': { multiplier: 0.975, ratePerSqft: 1780, state: 'Tamil Nadu' },
  'Hyderabad': { multiplier: 0.953, ratePerSqft: 1740, state: 'Telangana' },
  'Delhi NCR': { multiplier: 1.068, ratePerSqft: 1950, state: 'Delhi' },
  'Mumbai Metro': { multiplier: 1.249, ratePerSqft: 2280, state: 'Maharashtra' },
  'Pune': { multiplier: 1.019, ratePerSqft: 1860, state: 'Maharashtra' },
  'Kolkata': { multiplier: 0.926, ratePerSqft: 1690, state: 'West Bengal' },
};

export default function CostEstimationView({ costEstimate }: CostEstimationViewProps) {
  const [selectedCity, setSelectedCity] = useState<string>(costEstimate.location || 'Bengaluru');
  const [contingencyBuffer, setContingencyBuffer] = useState<number>(10); // 5% to 20%
  const [contractorMargin, setContractorMargin] = useState<number>(12); // 8% to 20%
  const [qualityGrade, setQualityGrade] = useState<'Standard' | 'Premium' | 'Luxury'>('Standard');

  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  const formatLakhs = (val: number) => {
    if (val >= 10000000) return `₹${(val / 10000000).toFixed(2)} Cr`;
    if (val >= 100000) return `₹${(val / 100000).toFixed(2)} Lakh`;
    return formatINR(val);
  };

  // Adjust calculations dynamically based on selected city, quality, and margins
  const cityData = CITY_RATES[selectedCity] || CITY_RATES['Bengaluru'];
  const qualityFactor = qualityGrade === 'Standard' ? 1.0 : qualityGrade === 'Premium' ? 1.18 : 1.35;
  const marginMultiplier = (1 + (contingencyBuffer - 10) / 100) * (1 + (contractorMargin - 12) / 100);

  const adjustedBaseCost = costEstimate.total_rule_based_cost * cityData.multiplier * qualityFactor * marginMultiplier;
  const adjustedCostPerSqm = adjustedBaseCost / Math.max(1, costEstimate.builtup_area);
  const adjustedRatePerSqft = Math.round(adjustedCostPerSqm / 10.764);

  return (
    <div className="space-y-6">
      {/* 1. Header Card matching Landing Page */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-100 gap-4">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-red-50 border border-red-200 text-red-700 text-[11px] font-bold uppercase tracking-wider mb-2">
              <Calculator className="w-3.5 h-3.5 text-red-600" />
              5D BIM Construction Cost Model (IS 1200 / IS 456)
            </div>
            <h3 className="text-xl font-bold text-slate-950">
              Deterministic Engineering Cost Estimation & PWD Rates
            </h3>
            <p className="text-xs text-slate-500 mt-1 max-w-2xl">
              Connected directly to the 3D BIM structural element quantities and regional Schedule of Rates (DSR). Adjust city location, contingency buffers, and finishing tiers below.
            </p>
          </div>

          <div className="flex items-center gap-2">
            <span className="bg-slate-100 border border-slate-200 text-slate-700 px-3 py-1.5 rounded-xl text-xs font-semibold flex items-center gap-1.5">
              <MapPin className="w-3.5 h-3.5 text-red-600" />
              {selectedCity} PWD DSR
            </span>
            <span className="bg-red-50 border border-red-200 text-red-700 px-3 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1.5">
              <Award className="w-3.5 h-3.5 text-red-600" />
              {qualityGrade} Grade
            </span>
          </div>
        </div>

        {/* Highlight Metrics */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 pt-4">
          <div className="bg-slate-50 border border-slate-200/80 rounded-xl p-5 shadow-xs">
            <span className="text-xs text-slate-500 font-medium block mb-1">Total Estimated Cost ({selectedCity})</span>
            <div className="text-3xl font-black text-slate-950 tracking-tight">
              {formatLakhs(adjustedBaseCost)}
            </div>
            <span className="text-xs font-mono text-slate-500 mt-1 block">
              {formatINR(adjustedBaseCost)}
            </span>
          </div>

          <div className="bg-white border-2 border-red-500/80 rounded-xl p-5 shadow-sm shadow-red-500/10">
            <span className="text-xs text-red-600 font-bold uppercase tracking-wider block mb-1">Unit Rate per Sq. Ft</span>
            <div className="text-3xl font-black text-slate-950 tracking-tight">
              ₹{adjustedRatePerSqft.toLocaleString()} <span className="text-xs font-normal text-slate-500">/ sq.ft</span>
            </div>
            <span className="text-xs text-slate-500 mt-1 block">
              ≈ ₹{Math.round(adjustedCostPerSqm).toLocaleString()} / m² Built-Up
            </span>
          </div>

          <div className="bg-slate-50 border border-slate-200/80 rounded-xl p-5 shadow-xs">
            <span className="text-xs text-slate-500 font-medium block mb-1">Total Built-Up Area</span>
            <div className="text-3xl font-black text-slate-950 tracking-tight">
              {costEstimate.builtup_area} <span className="text-xs font-normal text-slate-500">m²</span>
            </div>
            <span className="text-xs text-slate-500 mt-1 block">
              ≈ {(costEstimate.builtup_area * 10.764).toFixed(0)} sq.ft living area
            </span>
          </div>
        </div>
      </div>

      {/* 2. Interactive Requirements & Regional Rate Adjuster */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm space-y-5">
        <h4 className="text-sm font-bold text-slate-900 flex items-center gap-2">
          <Sliders className="w-4 h-4 text-red-600" />
          Location & Contractual Requirements Modifiers
        </h4>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* City / Location Selector */}
          <div>
            <label className="text-xs font-bold text-slate-700 block mb-2">Regional Market & PWD DSR Rates</label>
            <div className="space-y-1.5">
              {Object.keys(CITY_RATES).map((city) => (
                <button
                  key={city}
                  onClick={() => setSelectedCity(city)}
                  className={`w-full px-3 py-2 rounded-xl text-xs font-semibold border flex items-center justify-between transition ${
                    selectedCity === city
                      ? 'bg-red-600 border-red-600 text-white shadow-xs'
                      : 'bg-slate-50 border-slate-200 text-slate-700 hover:bg-slate-100'
                  }`}
                >
                  <span className="flex items-center gap-1.5">
                    <MapPin className="w-3.5 h-3.5" />
                    {city}
                  </span>
                  <span className="font-mono text-[11px]">
                    {selectedCity === city ? 'Base Rate' : `₹${CITY_RATES[city].ratePerSqft}/sqft`}
                  </span>
                </button>
              ))}
            </div>
          </div>

          {/* Quality Grade & Specifications */}
          <div className="space-y-4">
            <div>
              <label className="text-xs font-bold text-slate-700 block mb-2">Specification Tier</label>
              <div className="grid grid-cols-3 gap-1.5">
                {(['Standard', 'Premium', 'Luxury'] as const).map((grade) => (
                  <button
                    key={grade}
                    onClick={() => setQualityGrade(grade)}
                    className={`py-2 text-center text-xs font-bold rounded-xl border transition ${
                      qualityGrade === grade
                        ? 'bg-slate-900 border-slate-900 text-white shadow-xs'
                        : 'bg-slate-50 border-slate-200 text-slate-700 hover:bg-slate-100'
                    }`}
                  >
                    {grade}
                  </button>
                ))}
              </div>
            </div>

            <div className="bg-slate-50 border border-slate-200 rounded-xl p-3.5 text-xs space-y-1.5">
              <span className="font-bold text-slate-900 block">Tier Inclusions:</span>
              <p className="text-slate-600 text-[11px] leading-relaxed">
                {qualityGrade === 'Standard' && 'Standard OPC 53 concrete, clay brick masonry, vitrified floor tiles, premium emulsion.'}
                {qualityGrade === 'Premium' && 'Fly-ash green AAC blocks, teakwood doors, hardwood flooring, branded CP fittings, exterior texture.'}
                {qualityGrade === 'Luxury' && 'Imported Italian marble, triple-glazed acoustic UPVC, home automation, designer sanitary ware.'}
              </p>
            </div>
          </div>

          {/* Contingency & Contractor Margin Sliders */}
          <div className="space-y-4">
            <div>
              <div className="flex justify-between text-xs font-bold text-slate-700 mb-1">
                <span>Site Contingency Buffer</span>
                <span className="text-red-600 font-mono">{contingencyBuffer}%</span>
              </div>
              <input
                type="range"
                min="5"
                max="20"
                step="1"
                value={contingencyBuffer}
                onChange={(e) => setContingencyBuffer(parseInt(e.target.value))}
                className="w-full accent-red-600 cursor-pointer"
              />
              <span className="text-[10px] text-slate-400 block mt-0.5">Covers unexpected geotechnical and material price fluctuations</span>
            </div>

            <div>
              <div className="flex justify-between text-xs font-bold text-slate-700 mb-1">
                <span>Contractor Margin & Overhead</span>
                <span className="text-red-600 font-mono">{contractorMargin}%</span>
              </div>
              <input
                type="range"
                min="8"
                max="20"
                step="1"
                value={contractorMargin}
                onChange={(e) => setContractorMargin(parseInt(e.target.value))}
                className="w-full accent-red-600 cursor-pointer"
              />
              <span className="text-[10px] text-slate-400 block mt-0.5">Industry benchmark 10%–15% for residential turnkey contractors</span>
            </div>
          </div>
        </div>
      </div>

      {/* 3. Cost Composition Breakdown */}
      <div className="bg-white border border-slate-200/90 rounded-2xl p-6 shadow-sm space-y-4">
        <h4 className="text-sm font-bold text-slate-900">
          5D Structural Work-Package Breakdown
        </h4>
        <div className="space-y-3">
          {costEstimate.breakdown.map((item, idx) => {
            const adjustedItemAmount = item.amount * cityData.multiplier * qualityFactor * marginMultiplier;
            return (
              <div key={idx} className="bg-slate-50 border border-slate-200/80 p-3.5 rounded-xl">
                <div className="flex items-center justify-between text-xs mb-1.5">
                  <span className="font-bold text-slate-800">{item.category}</span>
                  <div className="flex items-center gap-3">
                    <span className="font-mono text-slate-500 font-semibold">{item.percentage}%</span>
                    <span className="font-mono font-bold text-slate-950">{formatINR(adjustedItemAmount)}</span>
                  </div>
                </div>
                <div className="w-full bg-slate-200 rounded-full h-2 overflow-hidden">
                  <div
                    className="bg-red-600 h-2 rounded-full transition-all duration-500"
                    style={{ width: `${Math.min(100, item.percentage)}%` }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
