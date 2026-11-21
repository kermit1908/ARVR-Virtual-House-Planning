'use client';

import React, { useState } from 'react';
import { QTOResponse, BOQResponse } from '../../types';
import { FileSpreadsheet, Layers, Info, Download, CheckCircle2 } from 'lucide-react';

interface QuantitiesTableProps {
  qto: QTOResponse;
  boq: BOQResponse;
}

export default function QuantitiesTable({ qto, boq }: QuantitiesTableProps) {
  const [activeTab, setActiveTab] = useState<'qto' | 'boq'>('qto');

  const formatINR = (val: number) => {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(val);
  };

  const downloadCSV = () => {
    const rows = activeTab === 'qto' 
      ? [
          ['Item Code', 'Category', 'Description', 'Raw Qty', 'Wastage %', 'Net Qty', 'Unit', 'Calculation Basis'],
          ...qto.items.map(i => [i.item_code, i.category, `"${i.description}"`, i.raw_quantity, i.wastage_percent, i.net_quantity, i.unit, `"${i.calculation_basis}"`])
        ]
      : [
          ['Item ID', 'Work Package', 'Description', 'Quantity', 'Unit', 'Unit Rate (INR)', 'Amount (INR)'],
          ...boq.items.map(i => [i.item_id, i.category, `"${i.description}"`, i.quantity, i.unit, i.unit_rate, i.amount])
        ];

    const csvContent = 'data:text/csv;charset=utf-8,' + rows.map(e => e.join(',')).join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `smartbuild_${activeTab}_report.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="bg-white border border-slate-200/90 rounded-2xl p-6 text-slate-800 shadow-sm space-y-6">
      {/* Header and Switcher */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-100 gap-4">
        <div>
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-red-50 border border-red-200 text-red-700 text-[11px] font-bold uppercase tracking-wider mb-2">
            <Layers className="w-3.5 h-3.5 text-red-600" />
            Standard Quantity Engine
          </div>
          <h3 className="text-xl font-bold text-slate-950">
            Automated Quantity Take-Off (QTO) & Bill of Quantities (BOQ)
          </h3>
          <p className="text-xs text-slate-500 mt-1">
            Deterministic quantities extracted from the BIM model following Indian Standards (IS 1200 / IS 456).
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="flex bg-slate-100 p-1 rounded-xl border border-slate-200">
            <button
              onClick={() => setActiveTab('qto')}
              className={`px-4 py-1.5 rounded-lg text-xs font-bold transition ${
                activeTab === 'qto' ? 'bg-red-600 text-white shadow-xs' : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Quantity Take-Off (QTO)
            </button>
            <button
              onClick={() => setActiveTab('boq')}
              className={`px-4 py-1.5 rounded-lg text-xs font-bold transition ${
                activeTab === 'boq' ? 'bg-red-600 text-white shadow-xs' : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              Bill of Quantities (BOQ)
            </button>
          </div>

          <button
            onClick={downloadCSV}
            className="p-2 border border-slate-200 hover:bg-slate-50 text-slate-700 rounded-xl transition text-xs font-semibold flex items-center gap-1.5"
            title="Download CSV Spreadsheet"
          >
            <Download className="w-4 h-4 text-red-600" />
            <span className="hidden sm:inline">Export CSV</span>
          </button>
        </div>
      </div>

      {activeTab === 'qto' ? (
        <div className="space-y-4">
          <div className="overflow-x-auto rounded-xl border border-slate-200">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-50 text-slate-700 uppercase font-bold text-[10px] tracking-wider border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Item Code</th>
                  <th className="py-3 px-4">Category</th>
                  <th className="py-3 px-4">Description</th>
                  <th className="py-3 px-4 text-right">Raw Qty</th>
                  <th className="py-3 px-4 text-right">Wastage</th>
                  <th className="py-3 px-4 text-right">Net Qty</th>
                  <th className="py-3 px-4">Unit</th>
                  <th className="py-3 px-4">Basis (IS 1200)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-slate-700">
                {qto.items.map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-50/80 transition">
                    <td className="py-3 px-4 font-mono font-bold text-red-700">{item.item_code}</td>
                    <td className="py-3 px-4 font-semibold text-slate-900">{item.category}</td>
                    <td className="py-3 px-4 max-w-xs">{item.description}</td>
                    <td className="py-3 px-4 text-right font-mono">{item.raw_quantity.toFixed(2)}</td>
                    <td className="py-3 px-4 text-right font-mono text-slate-500">+{item.wastage_percent}%</td>
                    <td className="py-3 px-4 text-right font-mono font-bold text-slate-950">{item.net_quantity.toFixed(2)}</td>
                    <td className="py-3 px-4 text-slate-500">{item.unit}</td>
                    <td className="py-3 px-4 text-[11px] text-slate-500">{item.calculation_basis}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="overflow-x-auto rounded-xl border border-slate-200">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-50 text-slate-700 uppercase font-bold text-[10px] tracking-wider border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Item ID</th>
                  <th className="py-3 px-4">Work Package</th>
                  <th className="py-3 px-4">Description</th>
                  <th className="py-3 px-4 text-right">Qty</th>
                  <th className="py-3 px-4">Unit</th>
                  <th className="py-3 px-4 text-right">Rate (INR)</th>
                  <th className="py-3 px-4 text-right">Amount (INR)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-slate-700">
                {boq.items.map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-50/80 transition">
                    <td className="py-3 px-4 font-mono font-bold text-red-700">{item.item_id}</td>
                    <td className="py-3 px-4 font-semibold text-slate-900">{item.category}</td>
                    <td className="py-3 px-4 text-slate-600 max-w-xs">{item.description}</td>
                    <td className="py-3 px-4 text-right font-mono">{item.quantity.toFixed(2)}</td>
                    <td className="py-3 px-4 text-slate-500">{item.unit}</td>
                    <td className="py-3 px-4 text-right font-mono font-semibold">₹{item.unit_rate.toLocaleString()}</td>
                    <td className="py-3 px-4 text-right font-mono font-bold text-slate-950">{formatINR(item.amount)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-slate-50 border-t-2 border-slate-200 text-slate-900 font-bold">
                <tr>
                  <td colSpan={6} className="py-3.5 px-4 text-right uppercase text-xs">Grand Total BOQ Estimate:</td>
                  <td className="py-3.5 px-4 text-right text-base text-red-600 font-black font-mono">
                    {formatINR(boq.grand_total)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
