'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api } from '../../lib/api';
import { Home, UserPlus, AlertCircle, ArrowRight } from 'lucide-react';

export default function SignupPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await api.signup(email, password, fullName);
      router.push('/dashboard');
    } catch (err: any) {
      setError(err.message || 'Failed to register account.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 bg-architectural-grid text-slate-900 flex flex-col justify-center py-12 px-6 lg:px-8 font-sans">
      <div className="sm:mx-auto sm:w-full sm:max-w-md text-center">
        <Link href="/" className="inline-flex items-center gap-2.5 mb-4 group">
          <div className="w-11 h-11 rounded-xl bg-red-600 flex items-center justify-center shadow-md shadow-red-600/25 group-hover:scale-105 transition">
            <Home className="w-5 h-5 text-white" />
          </div>
          <span className="font-extrabold text-2xl tracking-tight text-slate-950">SMARTBUILD</span>
        </Link>
        <h2 className="text-2xl font-black text-slate-950 tracking-tight">Create your account</h2>
        <p className="mt-1 text-xs text-slate-500">
          Register to build 3D BIM models, run automated QTO, and analyze ML cost predictions.
        </p>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-white border border-slate-200/90 py-8 px-6 shadow-xl shadow-slate-200/50 rounded-2xl sm:px-10">
          {error && (
            <div className="mb-4 bg-red-50 border border-red-200 p-3 rounded-xl text-xs text-red-700 flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-red-600 shrink-0" />
              <span>{error}</span>
            </div>
          )}

          <form className="space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1.5">
                Full Name
              </label>
              <input
                type="text"
                required
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="Architect Nikhil"
                className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1.5">
                Email Address
              </label>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="architect@university.edu"
                className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1.5">
                Password (min. 6 characters)
              </label>
              <input
                type="password"
                required
                minLength={6}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full bg-slate-50 border border-slate-300 rounded-xl px-3.5 py-2.5 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:bg-white focus:border-red-600 focus:ring-1 focus:ring-red-600 transition"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full mt-2 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white font-bold py-3 px-4 rounded-xl text-xs shadow-md shadow-red-600/25 transition flex items-center justify-center gap-2"
            >
              {loading ? 'Creating Account...' : 'Get Started'}
              <ArrowRight className="w-4 h-4" />
            </button>
          </form>

          <div className="mt-6 pt-5 border-t border-slate-100 text-center text-xs text-slate-500">
            <span>Already registered? </span>
            <Link href="/login" className="font-bold text-red-600 hover:underline">
              Sign in to account
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
