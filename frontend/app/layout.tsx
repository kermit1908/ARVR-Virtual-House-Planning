import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'SMARTBUILD AR/VR | AI-Enabled BIM Platform',
  description: 'AI-Enabled BIM-Based Residential Construction Visualization, Quantity Take-Off, Cost Estimation and Planning Platform',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-slate-950 text-slate-100 antialiased selection:bg-red-500 selection:text-white">
        {children}
      </body>
    </html>
  );
}
