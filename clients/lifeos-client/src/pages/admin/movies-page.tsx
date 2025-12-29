import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, Film, Tv } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createMovieSeries, updateMovieSeries, deleteMovieSeries, fetchMovieSeries } from '../../features/movieseries/api';
import { MovieSeries, MovieSeriesFormValues, MovieSeriesType, MovieSeriesStatus, MovieSeriesPlatform, MovieSeriesTypeLabels, MovieSeriesStatusLabels, MovieSeriesPlatformLabels } from '../../features/movieseries/types';
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
  type: z.nativeEnum(MovieSeriesType),
  platform: z.nativeEnum(MovieSeriesPlatform),
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
  const [selectedType, setSelectedType] = useState<MovieSeriesType | 'all'>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['movieseries', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchMovieSeries({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'createdDate', dir: 'desc' }
    })
  });

  const formMethods = useForm<MovieSeriesFormSchema>({
    resolver: zodResolver(movieSeriesSchema),
    defaultValues: {
      title: '',
      coverUrl: '',
      type: MovieSeriesType.Movie,
      platform: MovieSeriesPlatform.Netflix,
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
        type: editingMovie.type,
        platform: editingMovie.platform,
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
        type: MovieSeriesType.Movie,
        platform: MovieSeriesPlatform.Netflix,
        currentSeason: undefined,
        currentEpisode: undefined,
        status: MovieSeriesStatus.ToWatch,
        rating: undefined,
        personalNote: ''
      });
    }
  }, [editingMovie, formMethods]);

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
      type: values.type,
      platform: values.platform,
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
    if (selectedType === 'all') return data.items;
    return data.items.filter(movie => movie.type === selectedType);
  }, [data, selectedType]);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle>Film & Dizi Kütüphanesi</CardTitle>
            <p className="text-sm text-muted-foreground">
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
              <Button className="gap-2">
                <PlusCircle className="h-4 w-4" /> Yeni Ekle
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Yeni Film/Dizi</DialogTitle>
                <DialogDescription>İzlediğiniz film veya diziyi ekleyin.</DialogDescription>
              </DialogHeader>
              <form id="create-movie-form" onSubmit={onSubmit} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="movie-title">Başlık *</Label>
                    <Input id="movie-title" placeholder="Film/Dizi adı" {...formMethods.register('title')} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="movie-type">Tür</Label>
                    <select
                      id="movie-type"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('type', { valueAsNumber: true })}
                    >
                      {Object.entries(MovieSeriesTypeLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="movie-cover">Kapak URL</Label>
                  <Input id="movie-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
                  {formMethods.formState.errors.coverUrl && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.coverUrl.message}</p>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="movie-platform">Platform</Label>
                    <select
                      id="movie-platform"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('platform', { valueAsNumber: true })}
                    >
                      {Object.entries(MovieSeriesPlatformLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
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
                {formMethods.watch('type') === MovieSeriesType.Series && (
                  <div className="grid grid-cols-2 gap-4">
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
                <div className="grid grid-cols-2 gap-4">
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
              <DialogFooter>
                <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)}>
                  İptal
                </Button>
                <Button type="submit" form="create-movie-form" disabled={createMutation.isPending}>
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
                variant={selectedType === 'all' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedType('all')}
              >
                Tümü
              </Button>
              <Button
                variant={selectedType === MovieSeriesType.Movie ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedType(MovieSeriesType.Movie)}
              >
                <Film className="h-4 w-4 mr-2" />
                Filmler
              </Button>
              <Button
                variant={selectedType === MovieSeriesType.Series ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedType(MovieSeriesType.Series)}
              >
                <Tv className="h-4 w-4 mr-2" />
                Diziler
              </Button>
            </div>
            <Input
              placeholder="Film/Dizi ara..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setPageIndex(0);
              }}
              className="max-w-sm"
            />
          </div>
          {isLoading ? (
            <div className="text-center py-8">Yükleniyor...</div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {filteredMovies.map((movie) => (
                <Card key={movie.id} className="relative">
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <CardTitle className="text-lg">{movie.title}</CardTitle>
                        <div className="flex items-center gap-2 mt-2">
                          {movie.type === MovieSeriesType.Movie ? (
                            <Film className="h-4 w-4 text-primary" />
                          ) : (
                            <Tv className="h-4 w-4 text-primary" />
                          )}
                          <Badge variant="secondary">{MovieSeriesTypeLabels[movie.type]}</Badge>
                          <Badge variant="outline">{MovieSeriesStatusLabels[movie.status]}</Badge>
                        </div>
                      </div>
                      <div className="flex gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setEditingMovie(movie)}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setMovieToDelete(movie)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-2">
                    {movie.coverUrl && (
                      <img
                        src={movie.coverUrl}
                        alt={movie.title}
                        className="w-full h-48 object-cover rounded-md mb-2"
                      />
                    )}
                    <div className="text-sm text-muted-foreground">
                      <span>{MovieSeriesPlatformLabels[movie.platform]}</span>
                    </div>
                    {movie.type === MovieSeriesType.Series && (movie.currentSeason || movie.currentEpisode) && (
                      <div className="text-sm">
                        Sezon {movie.currentSeason || '?'}, Bölüm {movie.currentEpisode || '?'}
                      </div>
                    )}
                    {movie.rating && (
                      <div className="text-sm">
                        ⭐ {movie.rating}/10
                      </div>
                    )}
                    {movie.personalNote && (
                      <p className="text-sm text-muted-foreground line-clamp-2">{movie.personalNote}</p>
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
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Film/Diziyi Düzenle</DialogTitle>
            <DialogDescription>Film/Dizi bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-movie-form" onSubmit={onSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-movie-title">Başlık *</Label>
                <Input id="edit-movie-title" {...formMethods.register('title')} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-movie-type">Tür</Label>
                <select
                  id="edit-movie-type"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('type', { valueAsNumber: true })}
                >
                  {Object.entries(MovieSeriesTypeLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-movie-cover">Kapak URL</Label>
              <Input id="edit-movie-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
              {formMethods.formState.errors.coverUrl && (
                <p className="text-sm text-destructive">{formMethods.formState.errors.coverUrl.message}</p>
              )}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-movie-platform">Platform</Label>
                <select
                  id="edit-movie-platform"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('platform', { valueAsNumber: true })}
                >
                  {Object.entries(MovieSeriesPlatformLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
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
            {formMethods.watch('type') === MovieSeriesType.Series && (
              <div className="grid grid-cols-2 gap-4">
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
            <div className="grid grid-cols-2 gap-4">
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
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setEditingMovie(null)}>
              İptal
            </Button>
            <Button type="submit" form="edit-movie-form" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!movieToDelete} onOpenChange={(open) => !open && setMovieToDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Film/Diziyi Sil</DialogTitle>
            <DialogDescription>
              Bu film/diziyi silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek: <span className="font-medium">{movieToDelete?.title}</span>
          </p>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setMovieToDelete(null)}>
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => movieToDelete && deleteMutation.mutate(movieToDelete.id)}
            >
              {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

