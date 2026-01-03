import { NavLink } from 'react-router-dom';
import { FolderKanban, LayoutDashboard, Users, Shield, User, BookOpen, Gamepad2, Film, StickyNote, Wallet, Settings, ShoppingBag, Monitor, Tags, Music } from 'lucide-react';
import { cn } from '../../lib/utils';
import { usePermission } from '../../hooks/use-permission';
import { Permissions } from '../../lib/permissions';

// Menü grupları - mantıklı kategorilere ayrıldı
const menuGroups = [
  // Ana Sayfa
  {
    label: 'Ana Sayfa',
    items: [
      {
        to: '/admin/dashboard',
        label: 'Dashboard',
        icon: LayoutDashboard,
        requiredPermission: Permissions.DashboardView as string | undefined
      },
      {
        to: '/admin/profile',
        label: 'Profil',
        icon: User,
        requiredPermission: undefined // Herkes erişebilir
      }
    ]
  },
  // İçerik Yönetimi
  {
    label: 'İçerik Yönetimi',
    items: [
      {
        to: '/admin/books',
        label: 'Kitaplar',
        icon: BookOpen,
        requiredPermission: Permissions.BooksViewAll as string | undefined
      },
      {
        to: '/admin/games',
        label: 'Oyunlar',
        icon: Gamepad2,
        requiredPermission: Permissions.GamesViewAll as string | undefined
      },
      {
        to: '/admin/movies',
        label: 'Film & Diziler',
        icon: Film,
        requiredPermission: Permissions.MovieSeriesViewAll as string | undefined
      },
      {
        to: '/admin/notes',
        label: 'Notlar',
        icon: StickyNote,
        requiredPermission: Permissions.PersonalNotesViewAll as string | undefined
      },
      {
        to: '/admin/music',
        label: 'Müzik',
        icon: Music,
        requiredPermission: undefined // Herkes erişebilir
      }
    ]
  },
  // Finans
  {
    label: 'Finans',
    items: [
      {
        to: '/admin/wallet',
        label: 'Cüzdan',
        icon: Wallet,
        requiredPermission: Permissions.WalletTransactionsViewAll as string | undefined
      }
    ]
  },
  // Sistem Yönetimi
  {
    label: 'Sistem Yönetimi',
    items: [
      {
        to: '/admin/categories',
        label: 'Kategoriler',
        icon: FolderKanban,
        requiredPermission: Permissions.CategoriesViewAll as string | undefined
      },
      {
        to: '/admin/users',
        label: 'Kullanıcılar',
        icon: Users,
        requiredPermission: Permissions.UsersViewAll as string | undefined
      },
      {
        to: '/admin/roles',
        label: 'Roller & Yetkiler',
        icon: Shield,
        requiredPermission: Permissions.RolesViewAll as string | undefined
      },
      {
        to: '/admin/game-platforms',
        label: 'Oyun Platformları',
        icon: Gamepad2,
        requiredPermission: Permissions.GamePlatformsViewAll as string | undefined
      },
      {
        to: '/admin/game-stores',
        label: 'Oyun Mağazaları',
        icon: ShoppingBag,
        requiredPermission: Permissions.GameStoresViewAll as string | undefined
      },
      {
        to: '/admin/watch-platforms',
        label: 'İzleme Platformları',
        icon: Monitor,
        requiredPermission: Permissions.WatchPlatformsViewAll as string | undefined
      },
      {
        to: '/admin/movie-series-genres',
        label: 'Film/Dizi Türleri',
        icon: Tags,
        requiredPermission: Permissions.MovieSeriesGenresViewAll as string | undefined
      }
    ]
  }
];

export function AdminSidebar({ collapsed }: { collapsed: boolean }) {
  const { hasPermission } = usePermission();

  // Permission'a göre menü itemlarını filtrele
  const visibleGroups = menuGroups.map(group => ({
    ...group,
    items: group.items.filter((link) => 
      link.requiredPermission === undefined || hasPermission(link.requiredPermission)
    )
  })).filter(group => group.items.length > 0); // Boş grupları gizle

  return (
    <aside
      className={cn(
        'border-r bg-card transition-all duration-300 h-full flex flex-col',
        collapsed ? 'w-16' : 'w-64'
      )}
    >
      <div className="flex h-16 items-center justify-center border-b text-lg font-semibold shrink-0">
        {collapsed ? 'BP' : 'LifeOS'}
      </div>
      <nav className="flex-1 space-y-4 p-4 overflow-y-auto">
        {visibleGroups.map((group, groupIndex) => (
          <div key={groupIndex} className="space-y-1">
            {!collapsed && (
              <div className="px-3 py-2 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                {group.label}
              </div>
            )}
            {group.items.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-muted',
                    isActive ? 'bg-muted text-primary' : 'text-muted-foreground'
                  )
                }
              >
                <link.icon className="h-5 w-5 shrink-0" />
                {!collapsed && <span>{link.label}</span>}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>
    </aside>
  );
}
