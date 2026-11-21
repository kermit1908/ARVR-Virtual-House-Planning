'use client';

import React, { useEffect, useState } from 'react';
import { ScheduleResponse } from '../../types';
import { api } from '../../lib/api';
import { Calendar, Clock, GitCommit, HardHat, CheckCircle2 } from 'lucide-react';

interface GanttScheduleViewProps {
  projectId: string;
}

export default function GanttScheduleView({ projectId }: GanttScheduleViewProps) {
  const [schedule, setSchedule] = useState<ScheduleResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadSchedule() {
      try {
        const res = await api.getSchedule(projectId);
        setSchedule(res);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    }
    loadSchedule();
  }, [projectId]);

  if (loading) {
    return (
      <div className="bg-white border border-slate-200 rounded-2xl p-8 text-center text-slate-500 shadow-sm">
        Generating 4D construction schedule and critical path analysis...
      </div>
    );
  }

  if (!schedule) return null;

  const totalDays = schedule.total_duration_days;

  return (
    <div className="bg-white border border-slate-200 rounded-2xl p-6 text-slate-800 shadow-sm space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-slate-200 gap-4">
        <div>
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <Calendar className="w-5 h-5 text-red-600" />
            4D BIM Construction Schedule & Gantt Timeline
          </h3>
          <p className="text-xs text-slate-500 mt-1">
            Milestone sequencing and duration calculation using Critical Path Method (CPM) calibrated for residential construction.
          </p>
        </div>

        <div className="flex items-center gap-4 bg-slate-50 px-4 py-2 rounded-xl border border-slate-200 text-xs">
          <div>
            <span className="text-slate-500 block">Total Duration:</span>
            <span className="font-bold text-slate-900 text-sm">{schedule.total_duration_days} Days</span>
          </div>
          <span className="text-slate-300">|</span>
          <div>
            <span className="text-slate-500 block">Start Date:</span>
            <span className="font-medium text-slate-700">{schedule.start_date}</span>
          </div>
          <span className="text-slate-300">|</span>
          <div>
            <span className="text-slate-500 block">Est. Completion:</span>
            <span className="font-semibold text-emerald-600">{schedule.estimated_completion_date}</span>
          </div>
        </div>
      </div>

      {/* Gantt Chart Visualizer */}
      <div className="overflow-x-auto rounded-xl border border-slate-200 bg-slate-50 p-4">
        {/* Day Header Ruler */}
        <div className="grid grid-cols-12 gap-1 text-[10px] text-slate-500 font-mono pb-2 border-b border-slate-200 mb-3">
          <div className="col-span-4 font-semibold text-slate-700">Task Milestone</div>
          <div className="col-span-8 flex justify-between">
            <span>Day 0</span>
            <span>Day {Math.round(totalDays * 0.25)}</span>
            <span>Day {Math.round(totalDays * 0.5)}</span>
            <span>Day {Math.round(totalDays * 0.75)}</span>
            <span>Day {totalDays}</span>
          </div>
        </div>

        {/* Task Rows */}
        <div className="space-y-3">
          {schedule.tasks.map((task) => {
            const leftPercent = (task.start_day / totalDays) * 100;
            const widthPercent = Math.max(2, (task.duration_days / totalDays) * 100);

            let stageColor = 'bg-red-600';
            if (task.stage === 'Substructure') stageColor = 'bg-amber-600';
            if (task.stage === 'Superstructure') stageColor = 'bg-indigo-600';
            if (task.stage === 'Services') stageColor = 'bg-cyan-600';
            if (task.stage === 'Finishing') stageColor = 'bg-emerald-600';
            if (task.stage === 'Commissioning') stageColor = 'bg-purple-600';

            return (
              <div key={task.id} className="grid grid-cols-12 gap-2 items-center text-xs group">
                <div className="col-span-4 pr-2">
                  <div className="font-semibold text-slate-900 truncate">{task.name}</div>
                  <div className="text-[10px] text-slate-500 flex items-center gap-1.5">
                    <span className="font-mono text-slate-600">{task.id}</span>
                    <span>•</span>
                    <span>{task.duration_days} days</span>
                    <span>•</span>
                    <span className="text-slate-600 truncate">{task.assigned_crew}</span>
                  </div>
                </div>

                <div className="col-span-8 relative h-7 bg-white rounded-lg border border-slate-200 flex items-center">
                  <div
                    className={`absolute h-5 rounded-md ${stageColor} shadow-sm text-[10px] text-white font-medium flex items-center px-2 truncate transition-all duration-300`}
                    style={{
                      left: `${leftPercent}%`,
                      width: `${widthPercent}%`,
                    }}
                    title={`${task.name}: Days ${task.start_day} - ${task.end_day} (${task.duration_days} days)`}
                  >
                    {task.duration_days}d
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Legend */}
      <div className="flex flex-wrap items-center gap-4 text-xs text-slate-600 pt-2 border-t border-slate-200">
        <span className="font-semibold text-slate-800">Stages:</span>
        <span className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded bg-amber-600" /> Substructure</span>
        <span className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded bg-indigo-600" /> Superstructure (RCC & Walls)</span>
        <span className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded bg-cyan-600" /> MEP Services</span>
        <span className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded bg-emerald-600" /> Finishing & Flooring</span>
        <span className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded bg-purple-600" /> Commissioning</span>
      </div>
    </div>
  );
}
