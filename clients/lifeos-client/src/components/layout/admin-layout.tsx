import { useEffect, useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { AdminSidebar } from '../admin/sidebar';
import { AdminHeader } from '../admin/admin-header';
import { ScrollToTopButton } from '../ui/scroll-to-top-button';
import { cn } from '../../lib/utils';

export function AdminLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const [isMobile, setIsMobile] = useState(false);
  const location = useLocation();

  // Mobil modda otomatik olarak sidebar'ı collapse yap, desktop'ta otomatik aç
  useEffect(() => {
    const checkScreenSize = () => {
      const mobile = window.innerWidth < 768;
      setIsMobile(mobile);
      // md breakpoint (768px) altında otomatik collapse
      if (mobile) {
        setCollapsed(true);
      } else {
        // Desktop moda geçildiğinde otomatik aç
        setCollapsed(false);
      }
    };

    // İlk yüklemede kontrol et
    checkScreenSize();

    // Resize event listener ekle
    window.addEventListener('resize', checkScreenSize);

    // Cleanup
    return () => {
      window.removeEventListener('resize', checkScreenSize);
    };
  }, []);

  useEffect(() => {
    window.scrollTo({ top: 0, behavior: 'auto' });
  }, [location.pathname, location.search]);

  return (
    <div className="flex h-screen bg-muted/30 overflow-hidden">
      {/* Mobilde overlay, desktop'ta normal sidebar */}
      {isMobile && !collapsed && (
        <div
          className="fixed inset-0 z-40 bg-black/50 md:hidden"
          onClick={() => setCollapsed(true)}
        />
      )}
      {/* Desktop'ta normal sidebar, mobilde overlay */}
      <div
        className={cn(
          'transition-all duration-300',
          !isMobile && 'relative h-full',
          isMobile && collapsed && 'hidden',
          isMobile && !collapsed && 'fixed left-0 top-0 z-50 h-full shadow-lg'
        )}
      >
        {/* Mobilde sidebar her zaman açık görünümde (collapsed=false), desktop'ta collapsed state'e göre */}
        <AdminSidebar collapsed={isMobile ? false : collapsed} />
      </div>
      <div className="flex flex-1 flex-col w-full md:ml-0 h-full overflow-hidden">
        <AdminHeader
          isCollapsed={collapsed}
          onToggleSidebar={() => setCollapsed((prev) => !prev)}
        />
        <motion.main
          className="flex-1 p-4 sm:p-6 overflow-y-auto"
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.25 }}
        >
          <Outlet />
        </motion.main>
      </div>
      <ScrollToTopButton />
    </div>
  );
}
