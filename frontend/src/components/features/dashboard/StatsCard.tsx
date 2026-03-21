import { motion } from 'framer-motion';
import type { LucideIcon } from 'lucide-react';

interface StatsCardProps {
  label: string;
  value: string | number;
  icon: LucideIcon;
  index?: number;
}

export default function StatsCard({ label, value, icon: Icon, index = 0 }: StatsCardProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.1 }}
      className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-5 hover:border-[#3A3A3A] transition-colors"
    >
      <div className="flex items-center justify-between mb-3">
        <span className="text-sm text-[#555555]">{label}</span>
        <div className="w-8 h-8 rounded-lg bg-[#E8003D]/10 flex items-center justify-center">
          <Icon className="w-4 h-4 text-[#E8003D]" />
        </div>
      </div>
      <p className="text-2xl font-bold text-white">{value}</p>
    </motion.div>
  );
}
