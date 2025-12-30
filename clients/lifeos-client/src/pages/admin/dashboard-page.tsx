import { Link } from "react-router-dom";
import { motion } from "framer-motion";
import { useQuery } from "@tanstack/react-query";
import {
  FolderKanban,
  Users,
  Shield,
} from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { StatCard } from "../../components/dashboard/stat-card";
import { fetchStatistics } from "../../features/dashboard/api";

export function DashboardPage() {
  // ✅ Dashboard istatistikleri - sık değişen veri için staleTime override
  // Global staleTime (5 dakika) yerine 30 saniye kullanıyoruz
  const { data: stats, isLoading } = useQuery({
    queryKey: ["dashboard-statistics"],
    queryFn: fetchStatistics,
    staleTime: 30 * 1000, // 30 saniye (global 5 dakika yerine override)
    refetchInterval: 30000, // Her 30 saniyede bir güncelle
  });

  if (isLoading) {
    return (
      <div className="space-y-8">
        <Card className="p-6">
          <div className="flex items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
            <span className="ml-3 text-muted-foreground">Yükleniyor...</span>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <motion.div
        className="flex flex-col gap-4 rounded-xl border bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 p-6 shadow-sm lg:flex-row lg:items-center lg:justify-between"
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.25 }}
      >
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">
            Yönetim Paneline Hoş Geldiniz
          </h1>
          <p className="mt-2 max-w-xl text-sm text-muted-foreground">
            LifeOS içeriklerinizi kolayca yönetin, istatistiklerinizi takip
            edin.
          </p>
        </div>
      </motion.div>

      {/* İstatistik Kartları */}
      <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-4">
        <StatCard
          title="Toplam Kategoriler"
          value={stats?.totalCategories ?? 0}
          description="Aktif kategori sayısı"
          icon={FolderKanban}
          color="purple"
          delay={0}
        />
        <StatCard
          title="Toplam Kullanıcılar"
          value={stats?.totalUsers ?? 0}
          description="Kayıtlı kullanıcı sayısı"
          icon={Users}
          color="blue"
          delay={0.1}
        />
        <StatCard
          title="Toplam Roller"
          value={stats?.totalRoles ?? 0}
          description="Sistem rolleri"
          icon={Shield}
          color="green"
          delay={0.2}
        />
      </div>

      {/* Hızlı Aksiyonlar */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FolderKanban className="h-5 w-5 text-primary" />
            Hızlı Aksiyonlar
          </CardTitle>
          <CardDescription>Sık kullanılan işlemler</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/categories">
              <FolderKanban className="mr-2 h-4 w-4" />
              Kategorileri Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/users">
              <Users className="mr-2 h-4 w-4" />
              Kullanıcıları Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/roles">
              <Shield className="mr-2 h-4 w-4" />
              Rolleri Yönet
            </Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
