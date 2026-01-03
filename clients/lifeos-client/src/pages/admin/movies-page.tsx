import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, Film, Tv } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createMovieSeries, updateMovieSeries, deleteMovieSeries, fetchMovieSeries } from '../../features/movieseries/api';
import { MovieSeries, MovieSeriesFormValues, MovieSeriesStatus, MovieSeriesStatusLabels } from '../../features/movieseries/types';
import { getAllWatchPlatforms } from '../../features/watchplatforms/api';
import { getAllMovieSeriesGenres } from '../../features/movieseriesgenres/api';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '../../components/ui/dialog';
import { Badge } from '../../components/ui/badge';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { handleApiError, showApiResponseError } from '../../lib/api-error';

const movieSeriesSchema = z.object({
  title: z.string().min(2, 'Başlık en az 2 karakter olmalıdır').max(200, 'Başlık en fazla 200 karakter olabilir'),
  coverUrl: z.string().url('Geçerli bir URL girin').optional().or(z.literal('')),
  movieSeriesGenreId: z.string().uuid('Geçerli bir tür seçin'),
  watchPlatformId: z.string().uuid('Geçerli bir platform seçin'),
  currentSeason: z.number().min(1).optional().or(z.literal('')),
  currentEpisode: z.number().min(1).optional().or(z.literal('')),
  status: z.nativeEnum(MovieSeriesStatus),
  rating: z.number().min(1).max(10).optional().or(z.literal('')),
  personalNote: z.string().max(2000).optional().or(z.literal(''))
});

type MovieSeriesFormSchema = z.infer<typeof movieSeriesSchema>;

export function MoviesPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize] = useState(10);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingMovie, setEditingMovie] = useState<MovieSeries | null>(null);
  const [movieToDelete, setMovieToDelete] = useState<MovieSeries | null>(null);
  const [selectedGenre, setSelectedGenre] = useState<string | 'all'>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['movieseries', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchMovieSeries({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'createdDate', dir: 'desc' }
    })
  });

  const { data: watchPlatforms } = useQuery({
    queryKey: ['watch-platforms'],
    queryFn: getAllWatchPlatforms
  });

  const { data: movieSeriesGenres } = useQuery({
    queryKey: ['movie-series-genres'],
    queryFn: getAllMovieSeriesGenres
  });

  const formMethods = useForm<MovieSeriesFormSchema>({
    resolver: zodResolver(movieSeriesSchema),
    defaultValues: {
      title: '',
      coverUrl: '',
      movieSeriesGenreId: '',
      watchPlatformId: '',
      currentSeason: undefined,
      currentEpisode: undefined,
      status: MovieSeriesStatus.ToWatch,
      rating: undefined,
      personalNote: ''
    }
  });

  useEffect(() => {
    if (editingMovie) {
      formMethods.reset({
        title: editingMovie.title,
        coverUrl: editingMovie.coverUrl || '',
        movieSeriesGenreId: editingMovie.movieSeriesGenreId,
        watchPlatformId: editingMovie.watchPlatformId,
        currentSeason: editingMovie.currentSeason,
        currentEpisode: editingMovie.currentEpisode,
        status: editingMovie.status,
        rating: editingMovie.rating,
        personalNote: editingMovie.personalNote || ''
      });
    } else {
      formMethods.reset({
        title: '',
        coverUrl: '',
        movieSeriesGenreId: movieSeriesGenres?.[0]?.id || '',
        watchPlatformId: watchPlatforms?.[0]?.id || '',
        currentSeason: undefined,
        currentEpisode: undefined,
        status: MovieSeriesStatus.ToWatch,
        rating: undefined,
        personalNote: ''
      });
    }
  }, [editingMovie, formMethods, movieSeriesGenres, watchPlatforms]);

  const createMutation = useMutation({
    mutationFn: createMovieSeries,
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi eklenemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi eklendi');
      setIsCreateOpen(false);
      formMethods.reset();
      queryClient.invalidateQueries({ queryKey: ['movieseries'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: MovieSeriesFormValues) =>
      editingMovie ? updateMovieSeries(editingMovie.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi güncellenemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi güncellendi');
      setEditingMovie(null);
      queryClient.invalidateQueries({ queryKey: ['movieseries'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (movieId: string) => deleteMovieSeries(movieId),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi silinemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi silindi');
      setMovieToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['movieseries'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    const formData: MovieSeriesFormValues = {
      title: values.title,
      coverUrl: values.coverUrl || undefined,
      movieSeriesGenreId: values.movieSeriesGenreId,
      watchPlatformId: values.watchPlatformId,
      currentSeason: values.currentSeason || undefined,
      currentEpisode: values.currentEpisode || undefined,
      status: values.status,
      rating: values.rating || undefined,
      personalNote: values.personalNote || undefined
    };
    if (editingMovie) {
      await updateMutation.mutateAsync(formData);
    } else {
      await createMutation.mutateAsync(formData);
    }
  });

  const filteredMovies = useMemo(() => {
    if (!data?.items) return [];
    if (selectedGenre === 'all') return data.items;
    return data.items.filter(movie => movie.movieSeriesGenreId === selectedGenre);
  }, [data, selectedGenre]);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle className="text-lg sm:text-2xl">Film & Dizi Kütüphanesi</CardTitle>
            <p className="text-xs sm:text-sm text-muted-foreground">
              İzlediğiniz film ve dizileri takip edin.
            </p>
          </div>
          <Dialog
            open={isCreateOpen}
            onOpenChange={(open) => {
              setIsCreateOpen(open);
              if (!open && !editingMovie) {
                formMethods.reset();
              }
            }}
          >
            <DialogTrigger asChild>
              <Button className="gap-2 text-xs sm:text-sm">
                <PlusCircle className="h-3.5 w-3.5 sm:h-4 sm:w-4" /> <span className="hidden sm:inline">Yeni Ekle</span><span className="sm:hidden">Ekle</span>
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-full sm:max-w-2xl max-h-[90vh] overflow-y-auto mx-4">
              <DialogHeader>
                <DialogTitle>Yeni Film/Dizi</DialogTitle>
                <DialogDescription>İzlediğiniz film veya diziyi ekleyin.</DialogDescription>
              </DialogHeader>
              <form id="create-movie-form" onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="movie-title">Başlık *</Label>
                  <Input id="movie-title" placeholder="Film/Dizi adı" {...formMethods.register('title')} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="movie-cover">Kapak URL</Label>
                  <Input id="movie-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
                  {formMethods.formState.errors.coverUrl && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.coverUrl.message}</p>
                  )}
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="movie-genre">Tür</Label>
                    <select
                      id="movie-genre"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('movieSeriesGenreId')}
                    >
                      {movieSeriesGenres?.map(genre => (
                        <option key={genre.id} value={genre.id}>
                          {genre.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="movie-platform">Platform</Label>
                    <select
                      id="movie-platform"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('watchPlatformId')}
                    >
                      {watchPlatforms?.map(platform => (
                        <option key={platform.id} value={platform.id}>
                          {platform.name}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="movie-status">Durum</Label>
                    <select
                      id="movie-status"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('status', { valueAsNumber: true })}
                    >
                      {Object.entries(MovieSeriesStatusLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                {movieSeriesGenres?.find(g => g.id === formMethods.watch('movieSeriesGenreId'))?.name === 'Dizi' && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="movie-season">Sezon</Label>
                      <Input
                        id="movie-season"
                        type="number"
                        min="1"
                        {...formMethods.register('currentSeason', { valueAsNumber: true })}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="movie-episode">Bölüm</Label>
                      <Input
                        id="movie-episode"
                        type="number"
                        min="1"
                        {...formMethods.register('currentEpisode', { valueAsNumber: true })}
                      />
                    </div>
                  </div>
                )}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="movie-rating">Değerlendirme (1-10)</Label>
                    <Input
                      id="movie-rating"
                      type="number"
                      min="1"
                      max="10"
                      {...formMethods.register('rating', { valueAsNumber: true })}
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="movie-note">Kişisel Not</Label>
                  <textarea
                    id="movie-note"
                    className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    {...formMethods.register('personalNote')}
                  />
                </div>
              </form>
              <DialogFooter className="flex-col sm:flex-row gap-2">
                <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)} className="w-full sm:w-auto">
                  İptal
                </Button>
                <Button type="submit" form="create-movie-form" disabled={createMutation.isPending} className="w-full sm:w-auto">
                  {createMutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="flex gap-2">
              <Button
                variant={selectedGenre === 'all' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedGenre('all')}
              >
                Tümü
              </Button>
              {movieSeriesGenres?.map(genre => (
                <Button
                  key={genre.id}
                  variant={selectedGenre === genre.id ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setSelectedGenre(genre.id)}
                >
                  {genre.name === 'Film' ? <Film className="h-4 w-4 mr-2" /> : <Tv className="h-4 w-4 mr-2" />}
                  {genre.name}
                </Button>
              ))}
            </div>
            <Input
              placeholder="Film/Dizi ara..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setPageIndex(0);
              }}
              className="w-full sm:max-w-sm"
            />
          </div>
          {isLoading ? (
            <div className="text-center py-8">Yükleniyor...</div>
          ) : (
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-2 sm:gap-3">
              {filteredMovies.map((movie) => (
                <Card key={movie.id} className="relative flex flex-col overflow-hidden group">
                  {/* Resim üstte, poster formatı (dikey) */}
                  {movie.coverUrl && (
                    <div className="relative w-full aspect-[2/3] overflow-hidden bg-muted">
                      <img
                        src={movie.coverUrl}
                        alt={movie.title}
                        className="w-full h-full object-cover transition-transform group-hover:scale-105"
                      />
                      {/* Action buttons overlay */}
                      <div className="absolute top-1.5 right-1.5 flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button
                          variant="secondary"
                          size="icon"
                          className="h-6 w-6 bg-background/80 backdrop-blur-sm hover:bg-background"
                          onClick={() => setEditingMovie(movie)}
                        >
                          <Pencil className="h-3 w-3" />
                        </Button>
                        <Button
                          variant="secondary"
                          size="icon"
                          className="h-6 w-6 bg-background/80 backdrop-blur-sm hover:bg-destructive hover:text-destructive-foreground"
                          onClick={() => setMovieToDelete(movie)}
                        >
                          <Trash2 className="h-3 w-3" />
                        </Button>
                      </div>
                      {/* Rating badge overlay */}
                      {movie.rating && (
                        <div className="absolute bottom-1.5 left-1.5 bg-background/90 backdrop-blur-sm rounded-full px-1.5 py-0.5 text-[10px] sm:text-xs font-semibold">
                          ⭐ {movie.rating}
                        </div>
                      )}
                    </div>
                  )}
                  {/* İçerik */}
                  <CardContent className="p-2 sm:p-3 space-y-1.5 flex-1 flex flex-col">
                    {!movie.coverUrl && (
                      <div className="flex items-start justify-between gap-1.5 mb-1">
                        <div className="flex-1 min-w-0">
                          <CardTitle className="text-xs sm:text-sm font-semibold line-clamp-2 leading-tight">{movie.title}</CardTitle>
                        </div>
                        <div className="flex gap-0.5 shrink-0">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 sm:h-7 sm:w-7"
                            onClick={() => setEditingMovie(movie)}
                          >
                            <Pencil className="h-3 w-3 sm:h-3.5 sm:w-3.5" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 sm:h-7 sm:w-7"
                            onClick={() => setMovieToDelete(movie)}
                          >
                            <Trash2 className="h-3 w-3 sm:h-3.5 sm:w-3.5 text-destructive" />
                          </Button>
                        </div>
                      </div>
                    )}
                    {movie.coverUrl && (
                      <div className="min-w-0">
                        <CardTitle className="text-xs sm:text-sm font-semibold line-clamp-2 leading-tight">{movie.title}</CardTitle>
                      </div>
                    )}
                    <div className="flex items-center gap-1 flex-wrap">
                      {movie.movieSeriesGenreName === 'Film' ? (
                        <Film className="h-2.5 w-2.5 sm:h-3 sm:w-3 text-primary shrink-0" />
                      ) : (
                        <Tv className="h-2.5 w-2.5 sm:h-3 sm:w-3 text-primary shrink-0" />
                      )}
                      <Badge variant="secondary" className="text-[10px] px-1 py-0">{movie.movieSeriesGenreName}</Badge>
                      <Badge variant="outline" className="text-[10px] px-1 py-0">{MovieSeriesStatusLabels[movie.status]}</Badge>
                    </div>
                    <div className="text-[10px] sm:text-xs text-muted-foreground truncate">
                      <span>{movie.watchPlatformName}</span>
                    </div>
                    {movie.movieSeriesGenreName === 'Dizi' && (movie.currentSeason || movie.currentEpisode) && (
                      <div className="text-[10px] sm:text-xs font-medium">
                        S{movie.currentSeason || '?'} E{movie.currentEpisode || '?'}
                      </div>
                    )}
                    {!movie.coverUrl && movie.rating && (
                      <div className="text-[10px] sm:text-xs font-semibold">
                        ⭐ {movie.rating}/10
                      </div>
                    )}
                    {movie.personalNote && (
                      <p className="text-[10px] sm:text-xs text-muted-foreground line-clamp-2">{movie.personalNote}</p>
                    )}
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
          {filteredMovies.length === 0 && !isLoading && (
            <div className="text-center py-8 text-muted-foreground">
              Film/Dizi bulunamadı.
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog - Similar structure to create */}
      <Dialog open={!!editingMovie} onOpenChange={(open) => !open && setEditingMovie(null)}>
        <DialogContent className="max-w-full sm:max-w-2xl max-h-[90vh] overflow-y-auto mx-4">
          <DialogHeader>
            <DialogTitle>Film/Diziyi Düzenle</DialogTitle>
            <DialogDescription>Film/Dizi bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-movie-form" onSubmit={onSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="edit-movie-title">Başlık *</Label>
              <Input id="edit-movie-title" {...formMethods.register('title')} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-movie-cover">Kapak URL</Label>
              <Input id="edit-movie-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
              {formMethods.formState.errors.coverUrl && (
                <p className="text-sm text-destructive">{formMethods.formState.errors.coverUrl.message}</p>
              )}
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-movie-genre">Tür</Label>
                <select
                  id="edit-movie-genre"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('movieSeriesGenreId')}
                >
                  {movieSeriesGenres?.map(genre => (
                    <option key={genre.id} value={genre.id}>
                      {genre.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-movie-platform">Platform</Label>
                <select
                  id="edit-movie-platform"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('watchPlatformId')}
                >
                  {watchPlatforms?.map(platform => (
                    <option key={platform.id} value={platform.id}>
                      {platform.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-movie-status">Durum</Label>
                <select
                  id="edit-movie-status"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('status', { valueAsNumber: true })}
                >
                  {Object.entries(MovieSeriesStatusLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            {movieSeriesGenres?.find(g => g.id === formMethods.watch('movieSeriesGenreId'))?.name === 'Dizi' && (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="edit-movie-season">Sezon</Label>
                  <Input
                    id="edit-movie-season"
                    type="number"
                    min="1"
                    {...formMethods.register('currentSeason', { valueAsNumber: true })}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="edit-movie-episode">Bölüm</Label>
                  <Input
                    id="edit-movie-episode"
                    type="number"
                    min="1"
                    {...formMethods.register('currentEpisode', { valueAsNumber: true })}
                  />
                </div>
              </div>
            )}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-movie-rating">Değerlendirme</Label>
                <Input
                  id="edit-movie-rating"
                  type="number"
                  min="1"
                  max="10"
                  {...formMethods.register('rating', { valueAsNumber: true })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-movie-note">Kişisel Not</Label>
              <textarea
                id="edit-movie-note"
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                {...formMethods.register('personalNote')}
              />
            </div>
          </form>
          <DialogFooter className="flex-col sm:flex-row gap-2">
            <Button type="button" variant="ghost" onClick={() => setEditingMovie(null)} className="w-full sm:w-auto">
              İptal
            </Button>
            <Button type="submit" form="edit-movie-form" disabled={updateMutation.isPending} className="w-full sm:w-auto">
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!movieToDelete} onOpenChange={(open) => !open && setMovieToDelete(null)}>
        <DialogContent className="max-w-full sm:max-w-md mx-4">
          <DialogHeader>
            <DialogTitle>Film/Diziyi Sil</DialogTitle>
            <DialogDescription>
              Bu film/diziyi silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek: <span className="font-medium">{movieToDelete?.title}</span>
          </p>
          <DialogFooter className="flex-col sm:flex-row gap-2">
            <Button type="button" variant="ghost" onClick={() => setMovieToDelete(null)} className="w-full sm:w-auto">
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => movieToDelete && deleteMutation.mutate(movieToDelete.id)}
              className="w-full sm:w-auto"
            >
              {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

