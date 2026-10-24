'use client';

import React, { useState } from 'react';
import { QTOResponse, BOQResponse } from '../../types';
import { FileSpreadsheet, Layers, Info, Download } from 'lucide-react';

interface QuantitiesTableProps {
  qto: QTOResponse;
  boq: BOQResponse;
}

export default function QuantitiesTable({ qto, boq }: QuantitiesTableProps) {
  const [activeTab, setActiveTab] = useState<'qto' | 'boq'>('qto');

  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-200 shadow-xl space-y-6">
      {/* Header and Switcher */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-800 gap-4">
        <div>
          <h3 className="text-lg font-bold text-white flex items-center gap-2">
            <Layers className="w-5 h-5 text-red-500" />
            Automated Quantity Take-Off (QTO) & Bill of Quantities (BOQ)
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Deterministic engineering quantities extracted from the BIM model following Indian Standards (IS 1200 / IS 456).
          </p>
        </div>

        <div className="flex bg-slate-950 p-1 rounded-lg border border-slate-800">
          <button
            onClick={() => setActiveTab('qto')}
            className={`px-4 py-1.5 rounded-md text-xs font-semibold transition ${
              activeTab === 'qto' ? 'bg-red-600 text-white shadow' : 'text-slate-400 hover:text-white'
            }`}
          >
            Quantity Take-Off (QTO)
          </button>
          <button
            onClick={() => setActiveTab('boq')}
            className={`px-4 py-1.5 rounded-md text-xs font-semibold transition ${
              activeTab === 'boq' ? 'bg-red-600 text-white shadow' : 'text-slate-400 hover:text-white'
            }`}
          >
            Bill of Quantities (BOQ)
          </button>
        </div>
      </div>

      {activeTab === 'qto' ? (
        <div className="space-y-4">
          <div className="overflow-x-auto rounded-lg border border-slate-800">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-950 text-slate-400 uppercase font-semibold text-[11px] border-b border-slate-800">
                <tr>
                  <th className="py-3 px-4">Code</th>
                  <th className="py-3 px-4">Category</th>
                  <th className="py-3 px-4">Item Description</th>
                  <th className="py-3 px-4 text-right">Raw Qty</th>
                  <th className="py-3 px-4 text-right">Wastage</th>
                  <th className="py-3 px-4 text-right">Net Qty</th>
                  <th className="py-3 px-4">Unit</th>
                  <th className="py-3 px-4">Calculation Basis</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {qto.items.map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-800/50 transition">
                    <td className="py-2.5 px-4 font-mono text-red-400 font-medium">{item.item_code}</td>
                    <td className="py-2.5 px-4 font-semibold text-slate-300">{item.category}</td>
                    <td className="py-2.5 px-4 max-w-xs">{item.description}</td>
                    <td className="py-2.5 px-4 text-right font-mono text-slate-400">{item.raw_quantity.toLocaleString()}</td>
                    <td className="py-2.5 px-4 text-right font-mono text-amber-400">+{item.wastage_percent}%</td>
                    <td className="py-2.5 px-4 text-right font-mono text-emerald-400 font-bold">{item.net_quantity.toLocaleString()}</td>
                    <td className="py-2.5 px-4 font-medium text-slate-300">{item.unit}</td>
                    <td className="py-2.5 px-4 text-[11px] text-slate-400">{item.calculation_basis}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="bg-slate-950/60 p-4 rounded-lg border border-slate-800 text-xs text-slate-400 space-y-1">
            <span className="font-semibold text-slate-300 block mb-1 flex items-center gap-1.5">
              <Info className="w-3.5 h-3.5 text-red-500" />
              Engineering Assumptions & Measurement Rules:
            </span>
            {qto.assumptions.map((assump, aIdx) => (
              <p key={aIdx}>• {assump}</p>
            ))}
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="overflow-x-auto rounded-lg border border-slate-800">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-950 text-slate-400 uppercase font-semibold text-[11px] border-b border-slate-800">
                <tr>
                  <th className="py-3 px-4">Item ID</th>
                  <th className="py-3 px-4">Category</th>
                  <th className="py-3 px-4">Description</th>
                  <th className="py-3 px-4 text-right">Net Quantity</th>
                  <th className="py-3 px-4">Unit</th>
                  <th className="py-3 px-4 text-right">Unit Rate (INR)</th>
                  <th className="py-3 px-4 text-right">Amount (INR)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {boq.items.map((bItem, idx) => (
                  <tr key={idx} className="hover:bg-slate-800/50 transition">
                    <td className="py-2.5 px-4 font-mono text-red-400">{bItem.item_id}</td>
                    <td className="py-2.5 px-4 font-semibold text-slate-300">{bItem.category}</td>
                    <td className="py-2.5 px-4">{bItem.description}</td>
                    <td className="py-2.5 px-4 text-right font-mono font-medium">{bItem.quantity.toLocaleString()}</td>
                    <td className="py-2.5 px-4">{bItem.unit}</td>
                    <td className="py-2.5 px-4 text-right font-mono text-slate-400">₹{bItem.unit_rate.toLocaleString()}</td>
                    <td className="py-2.5 px-4 text-right font-mono font-bold text-white">{formatINR(bItem.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* BOQ Summary Card */}
          <div className="flex justify-end">
            <div className="bg-slate-950 border border-slate-800 rounded-xl p-4 w-full max-w-sm space-y-2 text-xs">
              <div className="flex justify-between text-slate-400">
                <span>Material Subtotal:</span>
                <span className="font-mono text-slate-200">{formatINR(boq.subtotal)}</span>
              </div>
              <div className="flex justify-between text-slate-400">
                <span>Project Contingency ({boq.contingency_percent}%):</span>
                <span className="font-mono text-amber-400">{formatINR(boq.contingency_amount)}</span>
              </div>
              <div className="flex justify-between text-sm font-bold text-white pt-2 border-t border-slate-800">
                <span>Total BOQ Estimate:</span>
                <span className="font-mono text-emerald-400">{formatINR(boq.grand_total)}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
