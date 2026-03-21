import { Outlet } from 'react-router-dom';
import Navbar from './Navbar';

export default function Sidebar() {
  return (
    <div className="min-h-screen bg-[#000000]">
      <Navbar />
      <main className="w-full">
        <Outlet />
      </main>
    </div>
  );
}
