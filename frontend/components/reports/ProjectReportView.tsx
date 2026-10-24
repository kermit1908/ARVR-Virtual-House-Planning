'use client';

import React, { useEffect, useState } from 'react';
import { ProjectReportResponse } from '../../types';
import { api } from '../../lib/api';
import { Printer, FileText, CheckCircle, Award, ShieldAlert, BookOpen, Download } from 'lucide-react';

interface ProjectReportViewProps {
  projectId: string;
}

export default function ProjectReportView({ projectId }: ProjectReportViewProps) {
  const [report, setReport] = useState<ProjectReportResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadReport() {
      try {
        const res = await api.getReport(projectId);
        setReport(res);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    }
    loadReport();
  }, [projectId]);

  if (loading) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-xl p-8 text-center text-slate-400">
        Compiling academic project report...
      </div>
    );
  }

  if (!report) return null;

  const handlePrint = () => {
    window.print();
  };

  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  return (
    <div className="space-y-6">
      {/* Floating Action Bar */}
      <div className="flex items-center justify-end gap-3 print:hidden">
        <a
          href="/SMARTBUILD_ARVR_Academic_Report.pdf"
          download="SMARTBUILD_ARVR_Academic_Report.pdf"
          className="bg-slate-800 hover:bg-slate-700 text-white font-semibold text-xs px-4 py-2.5 rounded-lg border border-slate-700 flex items-center gap-2 transition shadow-md"
        >
          <Download className="w-4 h-4 text-red-500" />
          Download IEEE Paper PDF
        </a>
        <button
          onClick={handlePrint}
          className="bg-red-600 hover:bg-red-500 text-white font-semibold text-xs px-4 py-2.5 rounded-lg shadow-lg flex items-center gap-2 transition"
        >
          <Printer className="w-4 h-4" />
          Print / Save as PDF
        </button>
      </div>

      {/* Printable Report Document Sheet */}
      <div className="bg-white text-slate-900 rounded-xl p-8 md:p-12 shadow-2xl border border-slate-200 print:shadow-none print:border-none print:p-0 space-y-8 font-sans">
        {/* Document Header */}
        <div className="border-b-2 border-slate-900 pb-6">
          <div className="flex flex-col sm:flex-row justify-between items-start gap-4">
            <div>
              <span className="text-xs uppercase tracking-widest font-black text-red-700">University Dissertation Project Report</span>
              <h1 className="text-3xl font-black text-slate-950 mt-1">SMARTBUILD AR/VR</h1>
              <p className="text-sm text-slate-600 font-medium mt-0.5">
                AI-Enabled BIM-Based Residential Construction Visualization, Quantity Take-Off, Cost Estimation and Planning Platform
              </p>
            </div>
            <div className="text-right text-xs text-slate-500">
              <span className="block font-bold text-slate-800">Generated At:</span>
              <span>{report.generated_at}</span>
              <span className="block mt-1 font-mono text-[11px] text-slate-400">Project ID: {report.project.id.slice(0, 16)}</span>
            </div>
          </div>
        </div>

        {/* 1. Project Overview & Specs */}
        <div>
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-300 pb-1 mb-3 uppercase tracking-wider">
            1. Project Overview & Building Parameters
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-xs">
            <div className="bg-slate-50 p-3 rounded border border-slate-200">
              <span className="text-slate-500 block">Project Title:</span>
              <span className="font-bold text-slate-900 text-sm">{report.project.name}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded border border-slate-200">
              <span className="text-slate-500 block">Regional Location:</span>
              <span className="font-bold text-slate-900 text-sm">{report.project.location}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded border border-slate-200">
              <span className="text-slate-500 block">Plot Dimensions:</span>
              <span className="font-bold text-slate-900 text-sm">{report.project.plot_width}m × {report.project.plot_length}m ({report.project.plot_area} m²)</span>
            </div>
            <div className="bg-slate-50 p-3 rounded border border-slate-200">
              <span className="text-slate-500 block">Total Built-Up Area:</span>
              <span className="font-bold text-red-700 text-sm">{report.bim_summary.builtup_area} m²</span>
            </div>
          </div>
        </div>

        {/* 2. BIM Digital Model Summary */}
        <div>
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-300 pb-1 mb-3 uppercase tracking-wider">
            2. BIM Single Source of Truth (SSOT) Model
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-xs text-slate-700">
            <div><strong>Storey Count:</strong> {report.bim_summary.floors_count} {report.bim_summary.floors_count === 1 ? 'Floor' : 'Floors'}</div>
            <div><strong>Floor Height:</strong> {report.bim_summary.floor_height} m</div>
            <div><strong>Wall Thickness:</strong> {report.bim_summary.wall_thickness} m</div>
            <div><strong>Total BIM Elements:</strong> {report.bim_summary.total_elements_count} Component Instances</div>
          </div>
        </div>

        {/* 3. Deterministic Quantity Take-Off (QTO) */}
        <div>
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-300 pb-1 mb-3 uppercase tracking-wider">
            3. Automated Quantity Take-Off (QTO)
          </h2>
          <table className="w-full text-left text-xs border border-slate-200">
            <thead className="bg-slate-100 text-slate-700">
              <tr>
                <th className="p-2 border">Code</th>
                <th className="p-2 border">Item</th>
                <th className="p-2 border text-right">Raw Qty</th>
                <th className="p-2 border text-right">Wastage</th>
                <th className="p-2 border text-right">Net Qty</th>
                <th className="p-2 border">Unit</th>
              </tr>
            </thead>
            <tbody>
              {report.qto.items.map((q, idx) => (
                <tr key={idx} className={idx % 2 === 0 ? 'bg-white' : 'bg-slate-50'}>
                  <td className="p-2 border font-mono">{q.item_code}</td>
                  <td className="p-2 border font-medium">{q.category} - {q.description}</td>
                  <td className="p-2 border text-right font-mono">{q.raw_quantity.toLocaleString()}</td>
                  <td className="p-2 border text-right font-mono">+{q.wastage_percent}%</td>
                  <td className="p-2 border text-right font-mono font-bold text-slate-950">{q.net_quantity.toLocaleString()}</td>
                  <td className="p-2 border">{q.unit}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* 4. 5D Cost Model & ML Prediction Comparison */}
        <div>
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-300 pb-1 mb-3 uppercase tracking-wider">
            4. 5D Cost Estimation & AI/ML Model Comparison
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <div className="bg-slate-50 p-4 rounded border border-slate-200">
              <span className="text-xs text-slate-500 block">5D Rule-Based Cost:</span>
              <span className="text-xl font-black text-slate-950">{formatINR(report.cost_estimate.total_rule_based_cost)}</span>
            </div>
            <div className="bg-slate-50 p-4 rounded border border-slate-200">
              <span className="text-xs text-slate-500 block">ML Predicted Cost ({report.ai_prediction.model_used}):</span>
              <span className="text-xl font-black text-red-700">{formatINR(report.ai_prediction.predicted_cost)}</span>
            </div>
            <div className="bg-slate-50 p-4 rounded border border-slate-200">
              <span className="text-xs text-slate-500 block">Variance (ML vs Rule-Based):</span>
              <span className="text-xl font-black text-slate-900">
                {report.ai_prediction.variance_amount >= 0 ? '+' : ''}{formatINR(report.ai_prediction.variance_amount)} ({report.ai_prediction.variance_percent}%)
              </span>
            </div>
          </div>
        </div>

        {/* 5. 4D Construction Schedule */}
        <div>
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-300 pb-1 mb-3 uppercase tracking-wider">
            5. Preliminary 4D Construction Schedule
          </h2>
          <p className="text-xs text-slate-600 mb-2">
            Target Completion: <strong>{report.schedule.total_duration_days} Days</strong> ({report.schedule.start_date} to {report.schedule.estimated_completion_date})
          </p>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-2 text-xs">
            {report.schedule.tasks.map((t) => (
              <div key={t.id} className="p-2 border border-slate-200 rounded bg-slate-50">
                <span className="font-mono text-[10px] text-slate-500 block">{t.id} • {t.duration_days} Days</span>
                <span className="font-semibold text-slate-800">{t.name}</span>
              </div>
            ))}
          </div>
        </div>

        {/* 6. Academic Research Context & Literature Justification */}
        <div className="bg-slate-50 p-5 rounded-lg border border-slate-200 text-xs text-slate-700 space-y-2">
          <h3 className="font-bold text-slate-900 uppercase text-xs flex items-center gap-1.5">
            <BookOpen className="w-4 h-4 text-red-700" />
            6. Academic Research Citations & Limitations
          </h3>
          <p>
            <strong>Research Foundation:</strong> Designed around BIM as a single source of truth for architectural visualization (Yigitbas et al., 2023), Web-based augmented reality validation (Sidani et al., 2021), and machine learning cost estimation using structural features (Jafary & Shojaei, 2026).
          </p>
          <p>
            <strong>System Limitations:</strong> Estimates are preliminary conceptual guidelines. Material unit rates are based on regional PWD schedules and do not replace certified structural engineering stamp or competitive contractor tenders.
          </p>
        </div>
      </div>
    </div>
  );
}
