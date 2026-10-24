'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Building2,
  CheckCircle2,
  ArrowRight,
  ShieldCheck,
  IndianRupee,
  Glasses,
  Search,
  SlidersHorizontal,
  Home,
  Layers,
  ChevronRight,
  Eye,
  Heart,
  Maximize2,
  Cpu,
  Calendar,
  FileText,
  Smartphone,
  ExternalLink
} from 'lucide-react';
import { HOUSE_TEMPLATES } from '../lib/houseTemplates';

export default function LandingPage() {
  const [selectedBhk, setSelectedBhk] = useState<number | 'all'>('all');
  const [plotSize, setPlotSize] = useState<number>(1500);
  const [budgetMax, setBudgetMax] = useState<number>(65);
  const [activeStyle, setActiveStyle] = useState<string>('all');
  const [previewModalImage, setPreviewModalImage] = useState<string | null>(null);

  // Filter templates based on user controls
  const filteredTemplates = HOUSE_TEMPLATES.filter((tmpl) => {
    if (selectedBhk !== 'all') {
      const bhkNum = parseInt(tmpl.bhk.charAt(0));
      if (bhkNum !== selectedBhk) return false;
    }
    return true;
  });

  return (
    <div className="min-h-screen bg-white text-slate-900 flex flex-col font-sans">
      {/* 1. Header Navigation */}
      <header className="border-b border-slate-200 bg-white/95 backdrop-blur-md sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          {/* Logo */}
          <div className="flex items-center gap-3">
            <Link href="/" className="flex items-center gap-2.5">
              <div className="w-10 h-10 rounded-xl bg-red-600 flex items-center justify-center shadow-md shadow-red-500/20">
                <Home className="w-5 h-5 text-white" />
              </div>
              <span className="font-extrabold text-xl tracking-tight text-slate-950">
                SMARTBUILD
              </span>
            </Link>
          </div>

          {/* Nav Links */}
          <nav className="hidden md:flex items-center gap-8 text-sm font-medium text-slate-600">
            <a href="#how-it-works" className="hover:text-red-600 transition flex items-center gap-1">
              How it works
            </a>
            <a href="#templates" className="hover:text-red-600 transition">
              Templates
            </a>
            <a href="#estimator" className="hover:text-red-600 transition">
              BOQ & Cost Estimator
            </a>
            <a href="#research" className="hover:text-red-600 transition">
              Academic Research
            </a>
          </nav>

          {/* Right Actions */}
          <div className="flex items-center gap-4">
            <button className="p-2 text-slate-500 hover:text-slate-800 transition" title="Search house plans">
              <Search className="w-5 h-5" />
            </button>
            <Link
              href="/login"
              className="text-sm font-semibold text-slate-700 hover:text-red-600 transition hidden sm:inline"
            >
              Sign in
            </Link>
            <Link
              href="/signup"
              className="bg-red-600 hover:bg-red-700 text-white text-sm font-semibold px-5 py-2.5 rounded-xl shadow-md shadow-red-600/25 transition flex items-center gap-2"
            >
              Start planning
              <ArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>
      </header>

      {/* 2. Hero Section */}
      <section className="bg-architectural-grid border-b border-slate-200/80 py-12 lg:py-20 px-6">
        <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          {/* Left Column: Headlines & Value Propositions */}
          <div className="lg:col-span-5 space-y-6">
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-red-50 border border-red-200 text-red-700 text-[11px] font-bold tracking-wider uppercase">
              <span className="w-2 h-2 rounded-full bg-red-600 animate-pulse" />
              Verified House Plans for a Smarter Tomorrow
            </div>

            <h1 className="text-4xl sm:text-5xl lg:text-[3.25rem] font-black text-slate-950 tracking-tight leading-[1.12]">
              A better starting point for every home.
            </h1>

            <p className="text-base text-slate-600 leading-relaxed">
              Choose from verified house templates, see accurate cost ranges, and experience your future home in immersive 3D, before you build.
            </p>

            <div className="flex flex-wrap items-center gap-4 pt-2">
              <Link
                href="/dashboard"
                className="bg-red-600 hover:bg-red-700 text-white font-bold px-7 py-3.5 rounded-xl text-sm shadow-lg shadow-red-600/25 transition flex items-center gap-2.5"
              >
                Start planning
                <ArrowRight className="w-4 h-4" />
              </Link>
              <a
                href="#templates"
                className="bg-white hover:bg-slate-50 text-slate-800 font-bold px-6 py-3.5 rounded-xl text-sm border border-slate-300 shadow-xs transition"
              >
                Browse templates
              </a>
            </div>

            {/* Value Props Row */}
            <div className="pt-8 border-t border-slate-200 grid grid-cols-1 sm:grid-cols-3 gap-5">
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-emerald-50 border border-emerald-200 flex items-center justify-center shrink-0">
                  <ShieldCheck className="w-4 h-4 text-emerald-600" />
                </div>
                <div>
                  <h4 className="text-xs font-bold text-slate-900">Verified plans</h4>
                  <p className="text-[11px] text-slate-500 mt-0.5 leading-snug">
                    Reviewed for standards and buildability
                  </p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-blue-50 border border-blue-200 flex items-center justify-center shrink-0">
                  <IndianRupee className="w-4 h-4 text-blue-600" />
                </div>
                <div>
                  <h4 className="text-xs font-bold text-slate-900">Accurate cost ranges</h4>
                  <p className="text-[11px] text-slate-500 mt-0.5 leading-snug">
                    Based on local rates and IS 1200 QTO
                  </p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-purple-50 border border-purple-200 flex items-center justify-center shrink-0">
                  <Glasses className="w-4 h-4 text-purple-600" />
                </div>
                <div>
                  <h4 className="text-xs font-bold text-slate-900">Immersive previews</h4>
                  <p className="text-[11px] text-slate-500 mt-0.5 leading-snug">
                    Explore in 3D, AR and VR at true scale
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Right Column: Interactive Template Explorer Widget (from Mockup) */}
          <div className="lg:col-span-7 bg-white border border-slate-200/90 rounded-2xl p-6 shadow-xl shadow-slate-200/50">
            {/* Top Widget Bar */}
            <div className="flex items-center justify-between pb-4 border-b border-slate-200">
              <div className="flex items-center gap-2">
                <span className="text-xs font-bold text-slate-900 uppercase tracking-wider">
                  Verified House Templates
                </span>
                <span className="text-xs px-2 py-0.5 rounded-full bg-slate-100 text-slate-600 font-semibold">
                  {filteredTemplates.length} Available
                </span>
              </div>
              <span className="text-xs text-slate-500">Sorted by Popularity</span>
            </div>

            {/* Filter Pills */}
            <div className="py-4 flex flex-wrap items-center justify-between gap-4 border-b border-slate-100">
              <div className="flex items-center gap-1.5">
                <span className="text-xs font-bold text-slate-500 mr-1.5">BHK:</span>
                {(['all', 2, 3, 4] as const).map((bhk) => (
                  <button
                    key={bhk}
                    onClick={() => setSelectedBhk(bhk)}
                    className={`px-3 py-1 rounded-lg text-xs font-bold transition ${
                      selectedBhk === bhk
                        ? 'bg-slate-950 text-white shadow-xs'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                    }`}
                  >
                    {bhk === 'all' ? 'All' : `${bhk} BHK`}
                  </button>
                ))}
              </div>

              <div className="flex items-center gap-3 text-xs text-slate-500">
                <span>Plot Area: <strong>{plotSize} sq ft</strong></span>
                <input
                  type="range"
                  min="800"
                  max="4000"
                  step="100"
                  value={plotSize}
                  onChange={(e) => setPlotSize(parseInt(e.target.value))}
                  className="accent-red-600 cursor-pointer w-28"
                />
              </div>
            </div>

            {/* Template Cards Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-4">
              {filteredTemplates.slice(0, 3).map((tmpl, idx) => (
                <div
                  key={tmpl.id}
                  className="bg-slate-50/70 border border-slate-200 rounded-xl overflow-hidden hover:border-red-500/50 hover:shadow-md transition flex flex-col group cursor-pointer"
                  onClick={() => setPreviewModalImage(tmpl.image)}
                >
                  <div className="relative aspect-video w-full bg-slate-200 overflow-hidden">
                    <img
                      src={tmpl.image}
                      alt={tmpl.name}
                      className="w-full h-full object-cover group-hover:scale-105 transition duration-300"
                    />
                    <div className="absolute top-2 left-2 bg-white/95 px-2 py-0.5 rounded text-[10px] font-bold text-red-600 flex items-center gap-1 shadow-xs">
                      <CheckCircle2 className="w-3 h-3 text-red-600" />
                      Verified plan
                    </div>
                  </div>

                  <div className="p-3.5 flex-1 flex flex-col justify-between">
                    <div>
                      <div className="flex items-center justify-between">
                        <h4 className="font-bold text-xs text-slate-900 truncate">{tmpl.name}</h4>
                        <span className="text-[10px] text-slate-400 font-mono">SB-0{idx + 1}</span>
                      </div>
                      <p className="text-[11px] text-slate-500 mt-1 line-clamp-2 leading-snug">
                        {tmpl.subtitle}
                      </p>
                    </div>

                    <div className="mt-3 pt-2.5 border-t border-slate-200/80 flex items-center justify-between text-[11px]">
                      <div>
                        <span className="text-slate-400 block text-[9px] uppercase">Built-Up</span>
                        <span className="font-bold text-slate-800">{tmpl.totalAreaSqm} m²</span>
                      </div>
                      <div className="text-right">
                        <span className="text-slate-400 block text-[9px] uppercase">Est. Cost Range</span>
                        <span className="font-bold text-red-600">
                          ₹{Math.round(tmpl.totalAreaSqm * 2.1)} - {Math.round(tmpl.totalAreaSqm * 2.8)} L
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Bottom Preview Strip from Mockup */}
            <div className="mt-4 p-3.5 bg-slate-900 text-white rounded-xl flex items-center justify-between gap-4">
              <div className="flex items-center gap-3">
                <div className="w-12 h-10 rounded bg-slate-800 overflow-hidden shrink-0 border border-slate-700">
                  <img
                    src={filteredTemplates[0]?.image || '/templates/template-1-l-shaped-deck.jpg'}
                    alt="Selected Plan"
                    className="w-full h-full object-cover"
                  />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs font-bold text-white">
                      {filteredTemplates[0]?.name || '3 BHK - Modern'}
                    </span>
                    <span className="text-[10px] text-emerald-400 font-semibold">• 100% Build-Ready</span>
                  </div>
                  <div className="text-[11px] text-slate-400 flex items-center gap-3 mt-0.5">
                    <span>Plot: 1,500 sq ft</span>
                    <span>Built-up: {filteredTemplates[0]?.totalAreaSqm || 145} m²</span>
                    <span className="text-red-400 font-semibold">₹32 - 45 Lakh</span>
                  </div>
                </div>
              </div>

              <Link
                href="/dashboard"
                className="bg-red-600 hover:bg-red-700 text-white text-xs font-bold px-4 py-2 rounded-lg transition shrink-0 flex items-center gap-1.5 shadow"
              >
                Plan this house
                <ChevronRight className="w-3.5 h-3.5" />
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* 3. "How it Works" 4-Step Process Bar */}
      <section id="how-it-works" className="py-14 border-b border-slate-200 bg-white px-6">
        <div className="max-w-7xl mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div className="flex items-start gap-3.5">
              <div className="w-8 h-8 rounded-full border-2 border-slate-900 flex items-center justify-center font-bold text-xs text-slate-900 shrink-0">
                1
              </div>
              <div>
                <h3 className="text-sm font-bold text-slate-950">Choose a plan</h3>
                <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                  Browse verified architectural templates that match your lifestyle and plot constraints.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3.5">
              <div className="w-8 h-8 rounded-full border-2 border-slate-900 flex items-center justify-center font-bold text-xs text-slate-900 shrink-0">
                2
              </div>
              <div>
                <h3 className="text-sm font-bold text-slate-950">Customize layout</h3>
                <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                  Adjust room sizes, floors, finishes, and partition walls parametrically in real time.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3.5">
              <div className="w-8 h-8 rounded-full border-2 border-slate-900 flex items-center justify-center font-bold text-xs text-slate-900 shrink-0">
                3
              </div>
              <div>
                <h3 className="text-sm font-bold text-slate-950">Preview in 3D & WebXR</h3>
                <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                  Walk through the house at 1.7m eye-level in VR or project it onto your ground in AR.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3.5">
              <div className="w-8 h-8 rounded-full border-2 border-slate-900 flex items-center justify-center font-bold text-xs text-slate-900 shrink-0">
                4
              </div>
              <div>
                <h3 className="text-sm font-bold text-slate-950">Generate BOQ & Estimate</h3>
                <p className="text-xs text-slate-500 mt-1 leading-relaxed">
                  Get accurate IS 1200 quantities, regional rates, and trained AI cost predictions.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 4. Dark CAD Blueprint & BOQ Cost Estimator Section (from Mockup) */}
      <section id="estimator" className="bg-[#0b1120] text-slate-100 py-16 lg:py-20 px-6 border-b border-slate-800">
        <div className="max-w-7xl mx-auto space-y-10">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-10 items-center">
            {/* Left Copy */}
            <div className="lg:col-span-4 space-y-5">
              <span className="text-[11px] font-bold tracking-widest text-red-400 uppercase">
                Real Plans. Real Quantities. Real Costs.
              </span>

              <h2 className="text-3xl sm:text-4xl font-black text-white tracking-tight leading-tight">
                Plans you can trust. <br />
                Costs you can plan for.
              </h2>

              <p className="text-sm text-slate-300 leading-relaxed">
                Every design comes with detailed area calculations, Bill of Quantities (BOQ) and realistic cost estimates so you can plan your budget with engineering precision.
              </p>

              <div className="pt-2">
                <Link
                  href="/dashboard"
                  className="bg-red-600 hover:bg-red-700 text-white font-bold px-6 py-3 rounded-xl text-xs transition inline-flex items-center gap-2 shadow-lg shadow-red-600/30"
                >
                  Explore templates
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </div>
            </div>

            {/* Center CAD Blueprint Simulation */}
            <div className="lg:col-span-5 bg-slate-950/90 border border-slate-800 rounded-2xl p-6 bg-blueprint-grid relative shadow-2xl overflow-hidden">
              <div className="flex items-center justify-between pb-3 border-b border-slate-800 text-[10px] text-slate-400 font-mono">
                <span>CAD SPECIFICATION: SB-2031</span>
                <span>GRID: 1:100 METRIC</span>
              </div>

              {/* Blueprint Floor Plan Diagram */}
              <div className="py-6 space-y-3 font-mono text-xs text-red-300">
                <div className="border border-red-500/40 p-4 rounded-lg bg-red-950/20">
                  <div className="flex justify-between items-center text-[10px] text-slate-400 border-b border-red-500/30 pb-2 mb-3">
                    <span>WIDTH: 12.0 m</span>
                    <span>LENGTH: 10.0 m</span>
                  </div>

                  <div className="grid grid-cols-2 gap-3">
                    <div className="border border-dashed border-red-500/40 p-2.5 rounded bg-red-950/40">
                      <span className="text-[11px] font-bold text-white block">Master Suite</span>
                      <span className="text-[10px] text-red-400">4.5m × 4.0m</span>
                    </div>
                    <div className="border border-dashed border-red-500/40 p-2.5 rounded bg-red-950/40">
                      <span className="text-[11px] font-bold text-white block">Kitchen & Dining</span>
                      <span className="text-[10px] text-red-400">4.0m × 3.5m</span>
                    </div>
                    <div className="border border-dashed border-red-500/40 p-2.5 rounded bg-red-950/40">
                      <span className="text-[11px] font-bold text-white block">Living Hall</span>
                      <span className="text-[10px] text-red-400">5.5m × 4.5m</span>
                    </div>
                    <div className="border border-dashed border-red-500/40 p-2.5 rounded bg-red-950/40">
                      <span className="text-[11px] font-bold text-white block">Ensuite & Laundry</span>
                      <span className="text-[10px] text-red-400">2.4m × 2.0m</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="flex items-center justify-between text-[11px] text-slate-400 pt-2 border-t border-slate-800">
                <span>Gross Floor Area: <strong>1,250 sq ft (116 m²)</strong></span>
                <span className="text-emerald-400 font-semibold">IS 1200 Compliant</span>
              </div>
            </div>

            {/* Right BOQ & Estimate Table (Directly from Mockup) */}
            <div className="lg:col-span-3 space-y-4">
              {/* BOQ Summary Card */}
              <div className="bg-slate-900/90 border border-slate-800 rounded-xl p-4 space-y-3">
                <div className="flex items-center justify-between border-b border-slate-800 pb-2 text-xs font-bold text-white">
                  <span>BOQ Summary</span>
                  <span className="text-[10px] text-slate-400 font-normal">IS 1200 Units</span>
                </div>

                <div className="space-y-2 text-xs">
                  <div className="flex justify-between text-slate-300">
                    <span>Foundation & Earthwork</span>
                    <span className="font-mono text-white">₹4,20,000</span>
                  </div>
                  <div className="flex justify-between text-slate-300">
                    <span>Brickwork & Masonry</span>
                    <span className="font-mono text-white">₹6,80,000</span>
                  </div>
                  <div className="flex justify-between text-slate-300">
                    <span>Flooring & Tiles</span>
                    <span className="font-mono text-white">₹5,90,000</span>
                  </div>
                  <div className="flex justify-between text-slate-300">
                    <span>Electrical & Plumbing</span>
                    <span className="font-mono text-white">₹4,50,000</span>
                  </div>
                  <div className="flex justify-between text-slate-300">
                    <span>Finishes (Painting, etc.)</span>
                    <span className="font-mono text-white">₹3,60,000</span>
                  </div>
                </div>

                <div className="pt-2 border-t border-slate-800 flex justify-between items-baseline">
                  <span className="text-xs font-bold text-slate-400">Est. Total Cost:</span>
                  <span className="text-base font-black text-red-500">₹25 - 40 Lakh</span>
                </div>
              </div>

              {/* Cost Split Specs */}
              <div className="bg-slate-900/90 border border-slate-800 rounded-xl p-4 text-xs space-y-2">
                <div className="flex justify-between text-slate-400">
                  <span>Materials (~60%):</span>
                  <span className="font-semibold text-white">₹24 - 39 Lakh</span>
                </div>
                <div className="flex justify-between text-slate-400">
                  <span>Labour (~40%):</span>
                  <span className="font-semibold text-white">₹16 - 26 Lakh</span>
                </div>
                <div className="flex justify-between text-slate-400">
                  <span>Cost per sq ft:</span>
                  <span className="font-semibold text-red-500">₹3,200 - 5,200</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 5. Complete Architectural Templates & Elevations Showcase */}
      <section id="templates" className="py-20 bg-slate-50/70 border-b border-slate-200 px-6">
        <div className="max-w-7xl mx-auto space-y-10">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div>
              <span className="text-xs font-bold text-red-600 uppercase tracking-wider block mb-1">
                Verified Architectural Plans
              </span>
              <h2 className="text-3xl font-black text-slate-950 tracking-tight">
                Architectural Plans & Visual Elevations
              </h2>
              <p className="text-sm text-slate-600 mt-1 max-w-2xl">
                Choose from contemporary single-level villas, duplex residences with open-tread glass staircases, central courtyard homes, and luxury estates.
              </p>
            </div>

            <Link
              href="/dashboard"
              className="bg-red-600 hover:bg-red-700 text-white font-bold px-5 py-2.5 rounded-xl text-xs transition flex items-center gap-1.5 shadow-sm shrink-0"
            >
              Start custom design
              <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          {/* Cards Grid of All 10 Templates */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {HOUSE_TEMPLATES.map((tmpl) => (
              <div
                key={tmpl.id}
                className="bg-white border border-slate-200 rounded-2xl overflow-hidden hover:border-red-500 hover:shadow-xl transition flex flex-col group"
              >
                {/* Image */}
                <div
                  className="relative aspect-video w-full bg-slate-100 overflow-hidden cursor-pointer"
                  onClick={() => setPreviewModalImage(tmpl.image)}
                >
                  <img
                    src={tmpl.image}
                    alt={tmpl.name}
                    className="w-full h-full object-cover group-hover:scale-105 transition duration-500"
                  />
                  <div className="absolute top-3 left-3 bg-white/95 backdrop-blur-xs px-2.5 py-1 rounded-lg text-xs font-bold text-red-600 shadow-sm">
                    {tmpl.bhk} • {tmpl.totalAreaSqm} m²
                  </div>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      setPreviewModalImage(tmpl.image);
                    }}
                    className="absolute top-3 right-3 p-1.5 rounded-lg bg-black/60 hover:bg-black/80 text-white transition opacity-0 group-hover:opacity-100"
                    title="Enlarge Image"
                  >
                    <Maximize2 className="w-4 h-4" />
                  </button>
                </div>

                {/* Content */}
                <div className="p-5 flex-1 flex flex-col justify-between">
                  <div>
                    <h3 className="font-bold text-base text-slate-900 tracking-tight">{tmpl.name}</h3>
                    <p className="text-xs text-red-600 font-semibold mt-0.5">{tmpl.subtitle}</p>
                    <p className="text-xs text-slate-500 mt-2 line-clamp-3 leading-relaxed">
                      {tmpl.description}
                    </p>

                    <div className="mt-4 flex flex-wrap gap-1.5">
                      {tmpl.config.floors[0].rooms.slice(0, 4).map((r, i) => (
                        <span
                          key={i}
                          className="text-[10px] bg-slate-100 text-slate-600 px-2 py-0.5 rounded font-medium"
                        >
                          {r.name}
                        </span>
                      ))}
                    </div>
                  </div>

                  <div className="mt-5 pt-4 border-t border-slate-100 flex items-center justify-between">
                    <div>
                      <span className="text-[10px] text-slate-400 block uppercase">Est. Cost Range</span>
                      <span className="font-bold text-sm text-slate-900">
                        ₹{Math.round(tmpl.totalAreaSqm * 2.1)} - {Math.round(tmpl.totalAreaSqm * 2.8)} Lakh
                      </span>
                    </div>

                    <Link
                      href="/dashboard"
                      className="bg-slate-900 hover:bg-red-600 text-white text-xs font-bold px-4 py-2 rounded-lg transition flex items-center gap-1.5"
                    >
                      Build in 3D
                      <ChevronRight className="w-3.5 h-3.5" />
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 6. Academic Research & University Project Credentials */}
      <section id="research" className="py-16 bg-white border-b border-slate-200 px-6">
        <div className="max-w-7xl mx-auto space-y-8">
          <div className="text-center max-w-3xl mx-auto space-y-3">
            <span className="text-xs font-bold text-red-600 uppercase tracking-widest">
              Academic & Engineering Foundations
            </span>
            <h2 className="text-3xl font-black text-slate-950 tracking-tight">
              Integrated 5D BIM & AI Research Architecture
            </h2>
            <p className="text-sm text-slate-600">
              Developed as a 5-credit software engineering dissertation project connecting BIM Single Source of Truth (SSOT), automated QTO (IS 1200), WebXR AR/VR, and supervised Machine Learning cost prediction.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-4">
            <div className="bg-slate-50 border border-slate-200 rounded-xl p-5 space-y-2">
              <div className="flex items-center gap-2 text-slate-900 font-bold text-sm">
                <Building2 className="w-4 h-4 text-red-600" />
                BIM + WebXR Immersive Visualization
              </div>
              <p className="text-xs text-slate-600 leading-relaxed">
                Citing <strong>Yigitbas et al. (2023)</strong> and <strong>Sidani et al. (2021)</strong>, integrating 1.7m eye-level VR walkthrough and AR surface plane projection directly in browser.
              </p>
            </div>

            <div className="bg-slate-50 border border-slate-200 rounded-xl p-5 space-y-2">
              <div className="flex items-center gap-2 text-slate-900 font-bold text-sm">
                <Cpu className="w-4 h-4 text-blue-600" />
                Machine Learning Cost Prediction
              </div>
              <p className="text-xs text-slate-600 leading-relaxed">
                Citing <strong>Jafary & Shojaei (2026)</strong>, trained Gradient Boosting model achieving <strong className="text-slate-900 font-mono">R² = 0.9715</strong> and <strong>MAE = ₹2.64 Lakhs</strong> across 1,200 project records.
              </p>
            </div>

            <div className="bg-slate-50 border border-slate-200 rounded-xl p-5 space-y-2">
              <div className="flex items-center gap-2 text-slate-900 font-bold text-sm">
                <Layers className="w-4 h-4 text-emerald-600" />
                Automated QTO & 4D CPM Scheduling
              </div>
              <p className="text-xs text-slate-600 leading-relaxed">
                Formulaic geometric calculus conforming to <strong>IS 1200 / IS 456</strong> standards with net wall deduction and 11-milestone forward/backward pass Critical Path Method.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* 7. Footer */}
      <footer className="bg-slate-950 text-slate-400 py-12 px-6">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-red-600 flex items-center justify-center">
              <Home className="w-4 h-4 text-white" />
            </div>
            <span className="font-extrabold text-white text-base">SMARTBUILD</span>
            <span className="text-xs text-slate-500">
              © {new Date().getFullYear()} University Capstone Research Project. MIT License.
            </span>
          </div>

          <div className="flex items-center gap-6 text-xs">
            <Link href="/login" className="hover:text-white transition">Sign in</Link>
            <Link href="/signup" className="hover:text-white transition">Create Account</Link>
            <Link href="/dashboard" className="text-red-400 hover:text-red-300 font-semibold transition">Project Workspace</Link>
          </div>
        </div>
      </footer>

      {/* Fullscreen Image Preview Modal */}
      {previewModalImage && (
        <div
          className="fixed inset-0 z-50 bg-black/90 backdrop-blur-md flex items-center justify-center p-4 cursor-pointer"
          onClick={() => setPreviewModalImage(null)}
        >
          <div className="relative max-w-5xl max-h-[90vh] bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden p-2">
            <button
              onClick={() => setPreviewModalImage(null)}
              className="absolute top-4 right-4 p-2 rounded-full bg-black/60 text-white hover:bg-black/90 transition z-10"
            >
              ✕
            </button>
            <img src={previewModalImage} alt="Full Render Preview" className="max-h-[85vh] w-auto mx-auto object-contain rounded-xl" />
          </div>
        </div>
      )}
    </div>
  );
}
