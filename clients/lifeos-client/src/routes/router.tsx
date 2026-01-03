import { Navigate, createBrowserRouter } from 'react-router-dom';
import { PublicLayout } from '../components/layout/public-layout';
import { HomePage } from '../pages/public/home-page';
import { LoginPage } from '../pages/public/login-page';
import { RegisterPage } from '../pages/public/register-page';
import { ProtectedRoute } from './protected-route';
import { AdminLayout } from '../components/layout/admin-layout';
import { DashboardPage } from '../pages/admin/dashboard-page';
import { CategoriesPage } from '../pages/admin/categories-page';
import { UsersPage } from '../pages/admin/users-page';
import { RolesPage } from '../pages/admin/roles-page';
import { ProfilePage } from '../pages/admin/profile-page';
import { BooksPage } from '../pages/admin/books-page';
import { GamesPage } from '../pages/admin/games-page';
import { MoviesPage } from '../pages/admin/movies-page';
import { NotesPage } from '../pages/admin/notes-page';
import { WalletPage } from '../pages/admin/wallet-page';
import { ReferenceDataPage } from '../pages/admin/reference-data-page';
import { GamePlatformsPage } from '../pages/admin/game-platforms-page';
import { GameStoresPage } from '../pages/admin/game-stores-page';
import { WatchPlatformsPage } from '../pages/admin/watch-platforms-page';
import { MovieSeriesGenresPage } from '../pages/admin/movie-series-genres-page';
import { MusicPage } from '../pages/admin/music-page';
import { MusicCallbackPage } from '../pages/public/music-callback-page';
import { ForbiddenPage } from '../pages/error/forbidden-page';
import { Permissions } from '../lib/permissions';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <PublicLayout />,
    children: [
      {
        index: true,
        element: <HomePage />
      },
      {
        path: 'login',
        element: <LoginPage />
      },
      {
        path: 'register',
        element: <RegisterPage />
      },
      {
        path: 'music/callback',
        element: <MusicCallbackPage />
      }
    ]
  },
  {
    path: '/admin',
    element: (
      <ProtectedRoute>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="dashboard" replace />
      },
      {
        path: 'dashboard',
        element: (
          <ProtectedRoute requiredPermission={Permissions.DashboardView}>
            <DashboardPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'categories',
        element: (
          <ProtectedRoute requiredPermission={Permissions.CategoriesViewAll}>
            <CategoriesPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'users',
        element: (
          <ProtectedRoute requiredPermission={Permissions.UsersViewAll}>
            <UsersPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'roles',
        element: (
          <ProtectedRoute requiredPermission={Permissions.RolesViewAll}>
            <RolesPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'profile',
        element: <ProfilePage />
      },
      {
        path: 'books',
        element: (
          <ProtectedRoute requiredPermission={Permissions.BooksViewAll}>
            <BooksPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'games',
        element: (
          <ProtectedRoute requiredPermission={Permissions.GamesViewAll}>
            <GamesPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'movies',
        element: (
          <ProtectedRoute requiredPermission={Permissions.MovieSeriesViewAll}>
            <MoviesPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'notes',
        element: (
          <ProtectedRoute requiredPermission={Permissions.PersonalNotesViewAll}>
            <NotesPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'wallet',
        element: (
          <ProtectedRoute requiredPermission={Permissions.WalletTransactionsViewAll}>
            <WalletPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'reference-data',
        element: (
          <ProtectedRoute>
            <ReferenceDataPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'game-platforms',
        element: (
          <ProtectedRoute requiredPermission={Permissions.GamePlatformsViewAll}>
            <GamePlatformsPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'game-stores',
        element: (
          <ProtectedRoute requiredPermission={Permissions.GameStoresViewAll}>
            <GameStoresPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'watch-platforms',
        element: (
          <ProtectedRoute requiredPermission={Permissions.WatchPlatformsViewAll}>
            <WatchPlatformsPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'movie-series-genres',
        element: (
          <ProtectedRoute requiredPermission={Permissions.MovieSeriesGenresViewAll}>
            <MovieSeriesGenresPage />
          </ProtectedRoute>
        )
      },
      {
        path: 'music',
        element: (
          <ProtectedRoute>
            <MusicPage />
          </ProtectedRoute>
        )
      }
    ]
  },
  {
    path: '/forbidden',
    element: <ForbiddenPage />
  },
  {
    path: '*',
    element: <Navigate to="/" replace />
  }
]);
