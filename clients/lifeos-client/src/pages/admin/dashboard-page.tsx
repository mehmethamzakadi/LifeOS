import { Link } from "react-router-dom";
import { motion } from "framer-motion";
import { useQuery } from "@tanstack/react-query";
import {
  FolderKanban,
  Users,
  Shield,
  BookOpen,
  Gamepad2,
  Film,
  StickyNote,
  Wallet,
  TrendingUp,
  ArrowRight,
  Calendar,
  DollarSign,
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
import {
  fetchStatistics,
  fetchRecentActivities,
  fetchFinancialSummary,
} from "../../features/dashboard/api";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import { format } from "date-fns";
import { tr } from "date-fns/locale";

const COLORS = ["#8884d8", "#82ca9d", "#ffc658", "#ff7300", "#0088fe"];

export function DashboardPage() {
  // Dashboard istatistikleri
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ["dashboard-statistics"],
    queryFn: fetchStatistics,
    staleTime: 30 * 1000,
    refetchInterval: 30000,
  });

  // Son aktiviteler
  const { data: activities, isLoading: activitiesLoading } = useQuery({
    queryKey: ["dashboard-recent-activities"],
    queryFn: fetchRecentActivities,
    staleTime: 60 * 1000,
  });

  // Finansal özet
  const { data: financial, isLoading: financialLoading } = useQuery({
    queryKey: ["dashboard-financial-summary"],
    queryFn: fetchFinancialSummary,
    staleTime: 60 * 1000,
  });

  const isLoading = statsLoading || activitiesLoading || financialLoading;

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

  // Grafik verileri
  const financialChartData = financial?.last6Months?.map((month) => ({
    name: month.month.split(" ")[0], // Sadece ay adı
    Gelir: month.income,
    Gider: month.expense,
    Net: month.net,
  })) || [];

  const bookStatusData = stats
    ? [
        { name: "Okunacak", value: stats.totalBooks - stats.totalBooksReading - stats.totalBooksCompleted },
        { name: "Okunuyor", value: stats.totalBooksReading },
        { name: "Tamamlandı", value: stats.totalBooksCompleted },
      ]
    : [];

  const gameStatusData = stats
    ? [
        { name: "Backlog", value: stats.totalGames - stats.totalGamesPlaying - stats.totalGamesCompleted },
        { name: "Oynanıyor", value: stats.totalGamesPlaying },
        { name: "Tamamlandı", value: stats.totalGamesCompleted },
      ]
    : [];

  const expenseCategoryData =
    financial?.topExpenseCategories?.map((cat) => ({
      name: cat.category,
      value: cat.amount,
    })) || [];

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

      {/* İstatistik Kartları - İlk Satır */}
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
        <StatCard
          title="Toplam Notlar"
          value={stats?.totalNotes ?? 0}
          description="Kişisel notlar"
          icon={StickyNote}
          color="yellow"
          delay={0.3}
        />
      </div>

      {/* İstatistik Kartları - İkinci Satır */}
      <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-4">
        <StatCard
          title="Toplam Kitaplar"
          value={stats?.totalBooks ?? 0}
          description={`${stats?.totalBooksReading ?? 0} okunuyor, ${stats?.totalBooksCompleted ?? 0} tamamlandı`}
          icon={BookOpen}
          color="blue"
          delay={0.4}
        />
        <StatCard
          title="Toplam Oyunlar"
          value={stats?.totalGames ?? 0}
          description={`${stats?.totalGamesPlaying ?? 0} oynanıyor, ${stats?.totalGamesCompleted ?? 0} tamamlandı`}
          icon={Gamepad2}
          color="green"
          delay={0.5}
        />
        <StatCard
          title="Toplam Film/Dizi"
          value={stats?.totalMovies ?? 0}
          description={`${stats?.totalMoviesWatching ?? 0} izleniyor, ${stats?.totalMoviesCompleted ?? 0} tamamlandı`}
          icon={Film}
          color="purple"
          delay={0.6}
        />
        <StatCard
          title="Bu Ay Net"
          value={`${((stats?.currentMonthIncome ?? 0) - (stats?.currentMonthExpense ?? 0)).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}`}
          description={`Gelir: ${(stats?.currentMonthIncome ?? 0).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}`}
          icon={Wallet}
          color={(stats?.currentMonthIncome ?? 0) - (stats?.currentMonthExpense ?? 0) >= 0 ? "green" : "red"}
          delay={0.7}
        />
      </div>

      {/* Grafikler ve Widget'lar */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Finansal Trend Grafiği */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              Son 6 Ay Finansal Trend
            </CardTitle>
            <CardDescription>Gelir ve gider karşılaştırması</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={financialChartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="Gelir"
                  stroke="#82ca9d"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="Gider"
                  stroke="#ff7300"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="Net"
                  stroke="#8884d8"
                  strokeWidth={2}
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Kitap Durumları */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <BookOpen className="h-5 w-5 text-primary" />
              Kitap Durumları
            </CardTitle>
            <CardDescription>Okuma durumu dağılımı</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={bookStatusData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={(props: any) => {
                    const percent = props.percent as number;
                    const name = props.name as string;
                    return `${name}: ${(percent * 100).toFixed(0)}%`;
                  }}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {bookStatusData.map((entry, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* İkinci Grafik Satırı */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Oyun Durumları */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Gamepad2 className="h-5 w-5 text-primary" />
              Oyun Durumları
            </CardTitle>
            <CardDescription>Oynama durumu dağılımı</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={gameStatusData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="value" fill="#8884d8" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Harcama Kategorileri */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <DollarSign className="h-5 w-5 text-primary" />
              En Çok Harcama Kategorileri
            </CardTitle>
            <CardDescription>Bu ay en çok harcama yapılan kategoriler</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={expenseCategoryData} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" />
                <YAxis dataKey="name" type="category" width={100} />
                <Tooltip />
                <Bar dataKey="value" fill="#ff7300" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Son Aktiviteler ve Finansal Özet */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Son Aktiviteler */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5 text-primary" />
              Son Aktiviteler
            </CardTitle>
            <CardDescription>En son eklenen içerikler</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Kitaplar */}
            {activities?.books && activities.books.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                  <BookOpen className="h-4 w-4" />
                  Son Kitaplar
                </h4>
                <div className="space-y-2">
                  {activities.books.slice(0, 3).map((book) => (
                    <div
                      key={book.id}
                      className="flex items-center justify-between p-2 rounded-lg border hover:bg-accent transition-colors"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium">{book.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {book.subtitle}
                        </p>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {format(new Date(book.createdDate), "dd MMM", {
                          locale: tr,
                        })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Oyunlar */}
            {activities?.games && activities.games.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                  <Gamepad2 className="h-4 w-4" />
                  Son Oyunlar
                </h4>
                <div className="space-y-2">
                  {activities.games.slice(0, 3).map((game) => (
                    <div
                      key={game.id}
                      className="flex items-center justify-between p-2 rounded-lg border hover:bg-accent transition-colors"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium">{game.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {game.subtitle}
                        </p>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {format(new Date(game.createdDate), "dd MMM", {
                          locale: tr,
                        })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Film/Diziler */}
            {activities?.movies && activities.movies.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                  <Film className="h-4 w-4" />
                  Son Film/Diziler
                </h4>
                <div className="space-y-2">
                  {activities.movies.slice(0, 3).map((movie) => (
                    <div
                      key={movie.id}
                      className="flex items-center justify-between p-2 rounded-lg border hover:bg-accent transition-colors"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium">{movie.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {movie.subtitle}
                        </p>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {format(new Date(movie.createdDate), "dd MMM", {
                          locale: tr,
                        })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Notlar */}
            {activities?.notes && activities.notes.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                  <StickyNote className="h-4 w-4" />
                  Son Notlar
                </h4>
                <div className="space-y-2">
                  {activities.notes.slice(0, 3).map((note) => (
                    <div
                      key={note.id}
                      className="flex items-center justify-between p-2 rounded-lg border hover:bg-accent transition-colors"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium">{note.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {note.subtitle || "Kategori yok"}
                        </p>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {format(new Date(note.createdDate), "dd MMM", {
                          locale: tr,
                        })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Cüzdan İşlemleri */}
            {activities?.walletTransactions && activities.walletTransactions.length > 0 && (
              <div>
                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                  <Wallet className="h-4 w-4" />
                  Son İşlemler
                </h4>
                <div className="space-y-2">
                  {activities.walletTransactions.slice(0, 3).map((transaction) => (
                    <div
                      key={transaction.id}
                      className="flex items-center justify-between p-2 rounded-lg border hover:bg-accent transition-colors"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium">{transaction.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {transaction.subtitle}
                        </p>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {format(new Date(transaction.createdDate), "dd MMM", {
                          locale: tr,
                        })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <Button variant="outline" className="w-full" asChild>
              <Link to="/admin/books">
                Tüm Aktiviteleri Gör
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
          </CardContent>
        </Card>

        {/* Finansal Özet */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Wallet className="h-5 w-5 text-primary" />
              Finansal Özet
            </CardTitle>
            <CardDescription>Bu ay ve toplam finansal durum</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="p-4 rounded-lg border bg-green-50 dark:bg-green-950/20">
                <p className="text-sm text-muted-foreground">Bu Ay Gelir</p>
                <p className="text-2xl font-bold text-green-600 dark:text-green-400">
                  {(financial?.currentMonthIncome ?? 0).toLocaleString("tr-TR", {
                    style: "currency",
                    currency: "TRY",
                  })}
                </p>
              </div>
              <div className="p-4 rounded-lg border bg-red-50 dark:bg-red-950/20">
                <p className="text-sm text-muted-foreground">Bu Ay Gider</p>
                <p className="text-2xl font-bold text-red-600 dark:text-red-400">
                  {(financial?.currentMonthExpense ?? 0).toLocaleString("tr-TR", {
                    style: "currency",
                    currency: "TRY",
                  })}
                </p>
              </div>
            </div>

            <div className="p-4 rounded-lg border bg-blue-50 dark:bg-blue-950/20">
              <p className="text-sm text-muted-foreground">Bu Ay Net</p>
              <p
                className={`text-2xl font-bold ${
                  (financial?.currentMonthNet ?? 0) >= 0
                    ? "text-green-600 dark:text-green-400"
                    : "text-red-600 dark:text-red-400"
                }`}
              >
                {(financial?.currentMonthNet ?? 0).toLocaleString("tr-TR", {
                  style: "currency",
                  currency: "TRY",
                })}
              </p>
            </div>

            <div className="pt-4 border-t">
              <p className="text-sm text-muted-foreground mb-2">
                Toplam Finansal Durum
              </p>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-sm">Toplam Gelir:</span>
                  <span className="text-sm font-medium text-green-600 dark:text-green-400">
                    {(financial?.totalIncome ?? 0).toLocaleString("tr-TR", {
                      style: "currency",
                      currency: "TRY",
                    })}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm">Toplam Gider:</span>
                  <span className="text-sm font-medium text-red-600 dark:text-red-400">
                    {(financial?.totalExpense ?? 0).toLocaleString("tr-TR", {
                      style: "currency",
                      currency: "TRY",
                    })}
                  </span>
                </div>
                <div className="flex justify-between pt-2 border-t">
                  <span className="text-sm font-semibold">Net Bakiye:</span>
                  <span
                    className={`text-sm font-bold ${
                      (financial?.totalNet ?? 0) >= 0
                        ? "text-green-600 dark:text-green-400"
                        : "text-red-600 dark:text-red-400"
                    }`}
                  >
                    {(financial?.totalNet ?? 0).toLocaleString("tr-TR", {
                      style: "currency",
                      currency: "TRY",
                    })}
                  </span>
                </div>
              </div>
            </div>

            <Button variant="outline" className="w-full" asChild>
              <Link to="/admin/wallet">
                Cüzdan İşlemlerini Gör
                <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
          </CardContent>
        </Card>
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
        <CardContent className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
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
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/books">
              <BookOpen className="mr-2 h-4 w-4" />
              Kitapları Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/games">
              <Gamepad2 className="mr-2 h-4 w-4" />
              Oyunları Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/movies">
              <Film className="mr-2 h-4 w-4" />
              Film/Dizileri Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/notes">
              <StickyNote className="mr-2 h-4 w-4" />
              Notları Yönet
            </Link>
          </Button>
          <Button variant="outline" className="w-full justify-start" asChild>
            <Link to="/admin/wallet">
              <Wallet className="mr-2 h-4 w-4" />
              Cüzdan İşlemleri
            </Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
