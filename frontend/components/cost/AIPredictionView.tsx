'use client';

import React, { useEffect, useState } from 'react';
import { AIPredictResponse, ModelComparisonResponse } from '../../types';
import { api } from '../../lib/api';
import { Cpu, TrendingUp, BarChart3, CheckCircle, RefreshCw, AlertCircle } from 'lucide-react';

interface AIPredictionViewProps {
  projectId: string;
  initialPrediction?: AIPredictResponse;
}

export default function AIPredictionView({ projectId, initialPrediction }: AIPredictResponse | any) {
  const [prediction, setPrediction] = useState<AIPredictResponse | null>(initialPrediction || null);
  const [comparison, setComparison] = useState<ModelComparisonResponse | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    async function loadData() {
      try {
        const comp = await api.getModelsComparison();
        setComparison(comp);

        if (!prediction) {
          setLoading(true);
          const pred = await api.predictCost(projectId);
          setPrediction(pred);
        }
      } catch (err) {
        console.error('Failed to load AI data:', err);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, [projectId]);

  const handleRerunPrediction = async () => {
    setLoading(true);
    try {
      const pred = await api.predictCost(projectId);
      setPrediction(pred);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const formatLakhs = (val: number) => {
    if (val >= 10000000) return `₹${(val / 10000000).toFixed(2)} Cr`;
    if (val >= 100000) return `₹${(val / 100000).toFixed(2)} Lakh`;
    return `₹${val.toLocaleString()}`;
  };

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-200 shadow-xl space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-800 gap-4">
        <div>
          <h3 className="text-lg font-bold text-white flex items-center gap-2">
            <Cpu className="w-5 h-5 text-indigo-400" />
            Machine Learning Construction Cost Prediction & Model Comparison
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Supervised machine-learning pipeline trained on BIM-derived residential project features, evaluated with 5-fold cross-validation.
          </p>
        </div>

        <button
          onClick={handleRerunPrediction}
          disabled={loading}
          className="bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-800 text-white text-xs font-semibold px-4 py-2 rounded-lg flex items-center gap-1.5 shadow transition"
        >
          <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
          Run Inference
        </button>
      </div>

      {prediction && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Predicted Cost */}
          <div className="bg-slate-950 border border-indigo-500/40 rounded-xl p-5 shadow-lg relative overflow-hidden">
            <div className="absolute top-0 right-0 w-24 h-24 bg-indigo-500/10 rounded-full blur-2xl" />
            <span className="text-xs text-indigo-400 font-semibold uppercase tracking-wider block mb-1">
              ML Predicted Cost ({prediction.model_used})
            </span>
            <div className="text-3xl font-black text-white tracking-tight">
              {formatLakhs(prediction.predicted_cost)}
            </div>
            <span className="text-xs font-mono text-slate-400 mt-1 block">
              95% Confidence Interval: {formatLakhs(prediction.confidence_lower)} - {formatLakhs(prediction.confidence_upper)}
            </span>
          </div>

          {/* Rule-Based Comparison */}
          <div className="bg-slate-950 border border-slate-800 rounded-xl p-5 shadow-lg">
            <span className="text-xs text-slate-400 font-semibold uppercase tracking-wider block mb-1">
              5D Rule-Based Estimate
            </span>
            <div className="text-3xl font-black text-slate-200 tracking-tight">
              {formatLakhs(prediction.rule_based_cost)}
            </div>
            <span className="text-xs font-mono text-slate-400 mt-1 block">
              Deterministic QTO x Unit Rate Tables
            </span>
          </div>

          {/* Variance Analysis */}
          <div className="bg-slate-950 border border-slate-800 rounded-xl p-5 shadow-lg">
            <span className="text-xs text-slate-400 font-semibold uppercase tracking-wider block mb-1">
              Empirical Variance
            </span>
            <div className={`text-3xl font-black tracking-tight ${prediction.variance_amount >= 0 ? 'text-amber-400' : 'text-emerald-400'}`}>
              {prediction.variance_amount >= 0 ? '+' : ''}{formatLakhs(prediction.variance_amount)}
            </div>
            <span className="text-xs font-mono text-slate-400 mt-1 block">
              {prediction.variance_percent >= 0 ? '+' : ''}{prediction.variance_percent}% delta vs rule-based calculation
            </span>
          </div>
        </div>
      )}

      {/* Model Comparison Academic Table */}
      {comparison && (
        <div className="space-y-4 pt-2">
          <div>
            <h4 className="text-sm font-semibold text-white flex items-center gap-2 mb-1">
              <BarChart3 className="w-4 h-4 text-red-500" />
              Academic Model Comparison ({comparison.dataset_size} Project Records)
            </h4>
            <p className="text-xs text-slate-400">
              Evaluation metrics across candidate regression architectures (Linear Regression, Random Forest, Gradient Boosting).
            </p>
          </div>

          <div className="overflow-x-auto rounded-lg border border-slate-800">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-950 text-slate-400 uppercase font-semibold text-[11px] border-b border-slate-800">
                <tr>
                  <th className="py-3 px-4">Architecture</th>
                  <th className="py-3 px-4 text-right">R² Score</th>
                  <th className="py-3 px-4 text-right">5-Fold CV R²</th>
                  <th className="py-3 px-4 text-right">MAE (INR)</th>
                  <th className="py-3 px-4 text-right">MAPE (%)</th>
                  <th className="py-3 px-4 text-right">RMSE (INR)</th>
                  <th className="py-3 px-4 text-center">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {comparison.models.map((m, idx) => {
                  const isBest = m.model_name === comparison.best_model;
                  return (
                    <tr key={idx} className={isBest ? 'bg-indigo-950/20' : 'hover:bg-slate-800/40'}>
                      <td className="py-3 px-4 font-semibold text-white flex items-center gap-2">
                        {m.model_name}
                        {isBest && <span className="px-2 py-0.5 rounded text-[10px] bg-indigo-500/20 text-indigo-300 border border-indigo-500/30 font-normal">Best Model</span>}
                      </td>
                      <td className="py-3 px-4 text-right font-mono font-bold text-red-400">{m.r2_score.toFixed(4)}</td>
                      <td className="py-3 px-4 text-right font-mono text-slate-300">{m.cv_r2_score.toFixed(4)}</td>
                      <td className="py-3 px-4 text-right font-mono text-slate-300">₹{m.mae_inr.toLocaleString()}</td>
                      <td className="py-3 px-4 text-right font-mono text-emerald-400 font-semibold">{m.mape_percent.toFixed(2)}%</td>
                      <td className="py-3 px-4 text-right font-mono text-slate-400">₹{m.rmse_inr.toLocaleString()}</td>
                      <td className="py-3 px-4 text-center">
                        {isBest ? (
                          <span className="text-emerald-400 font-semibold inline-flex items-center gap-1">
                            <CheckCircle className="w-3.5 h-3.5" /> Deployed
                          </span>
                        ) : (
                          <span className="text-slate-500">Benchmark</span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Top Feature Importance */}
          <div>
            <h5 className="text-xs font-semibold text-slate-300 mb-2">BIM Feature Importance Contributions</h5>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3">
              {Object.entries(comparison.top_features).slice(0, 8).map(([feat, imp], idx) => (
                <div key={idx} className="bg-slate-950 p-2.5 rounded-lg border border-slate-800 text-xs">
                  <div className="flex justify-between text-slate-400 mb-1">
                    <span className="truncate pr-1">{feat.replace('_', ' ')}</span>
                    <span className="font-mono text-red-400 font-bold">{imp}%</span>
                  </div>
                  <div className="w-full bg-slate-800 rounded-full h-1.5 overflow-hidden">
                    <div className="bg-red-500 h-1.5 rounded-full" style={{ width: `${Math.min(100, imp * 2.5)}%` }} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
