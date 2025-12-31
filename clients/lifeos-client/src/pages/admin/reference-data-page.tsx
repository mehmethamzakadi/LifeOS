import { useState, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2 } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { getAllGamePlatforms, createGamePlatform, updateGamePlatform, deleteGamePlatform } from '../../features/gameplatforms/api';
import { getAllGameStores, createGameStore, updateGameStore, deleteGameStore } from '../../features/gamestores/api';
import { getAllWatchPlatforms, createWatchPlatform, updateWatchPlatform, deleteWatchPlatform } from '../../features/watchplatforms/api';
import { getAllMovieSeriesGenres, createMovieSeriesGenre, updateMovieSeriesGenre, deleteMovieSeriesGenre } from '../../features/movieseriesgenres/api';
import { GamePlatform, GamePlatformFormValues } from '../../features/gameplatforms/types';
import { GameStore, GameStoreFormValues } from '../../features/gamestores/types';
import { WatchPlatform, WatchPlatformFormValues } from '../../features/watchplatforms/types';
import { MovieSeriesGenre, MovieSeriesGenreFormValues } from '../../features/movieseriesgenres/types';
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
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import toast from 'react-hot-toast';
import { handleApiError, showApiResponseError } from '../../lib/api-error';

const nameSchema = z.object({
  name: z.string().min(1, 'Ad boş olamaz').max(100, 'Ad en fazla 100 karakter olabilir')
});

type NameFormSchema = z.infer<typeof nameSchema>;

// GamePlatform Bölümü
function GamePlatformSection() {
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<GamePlatform | null>(null);
  const [itemToDelete, setItemToDelete] = useState<GamePlatform | null>(null);

  const { data: platforms, isLoading } = useQuery({
    queryKey: ['game-platforms'],
    queryFn: getAllGamePlatforms
  });

  const formMethods = useForm<NameFormSchema>({
    resolver: zodResolver(nameSchema),
    defaultValues: { name: '' }
  });

  useEffect(() => {
    if (editingItem) {
      formMethods.reset({ name: editingItem.name });
    } else {
      formMethods.reset({ name: '' });
    }
  }, [editingItem, formMethods]);

  const createMutation = useMutation({
    mutationFn: (values: GamePlatformFormValues) => createGamePlatform(values),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun platformu eklenemedi');
        return;
      }
      toast.success(result.message || 'Oyun platformu eklendi');
      setIsCreateOpen(false);
      queryClient.invalidateQueries({ queryKey: ['game-platforms'] });
    },
    onError: (error) => handleApiError(error, 'Oyun platformu eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: GamePlatformFormValues) =>
      editingItem ? updateGamePlatform(editingItem.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun platformu güncellenemedi');
        return;
      }
      toast.success(result.message || 'Oyun platformu güncellendi');
      setEditingItem(null);
      queryClient.invalidateQueries({ queryKey: ['game-platforms'] });
    },
    onError: (error) => handleApiError(error, 'Oyun platformu güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteGamePlatform(id),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun platformu silinemedi');
        return;
      }
      toast.success(result.message || 'Oyun platformu silindi');
      setItemToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['game-platforms'] });
    },
    onError: (error) => handleApiError(error, 'Oyun platformu silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    if (editingItem) {
      await updateMutation.mutateAsync({ name: values.name });
    } else {
      await createMutation.mutateAsync({ name: values.name });
    }
  });

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Oyun Platformları</CardTitle>
          <Dialog open={isCreateOpen && !editingItem} onOpenChange={(open) => {
            setIsCreateOpen(open);
            if (open) setEditingItem(null);
          }}>
            <DialogTrigger asChild>
              <Button size="sm" onClick={() => setEditingItem(null)}>
                <PlusCircle className="h-4 w-4 mr-2" />
                Ekle
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Yeni Oyun Platformu Ekle</DialogTitle>
                <DialogDescription>Oyun platformu bilgilerini girin</DialogDescription>
              </DialogHeader>
              <form onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="game-platform-name">Platform Adı</Label>
                  <Input
                    id="game-platform-name"
                    {...formMethods.register('name')}
                    placeholder="Örn: PC, PS5, Xbox"
                  />
                  {formMethods.formState.errors.name && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                  )}
                </div>
                <DialogFooter>
                  <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>
                    İptal
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="text-center py-4">Yükleniyor...</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ad</TableHead>
                <TableHead className="text-right">İşlemler</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {platforms && platforms.length > 0 ? (
                platforms.map((platform) => (
                  <TableRow key={platform.id}>
                    <TableCell>{platform.name}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setIsCreateOpen(false);
                            setEditingItem(platform);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setItemToDelete(platform)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={2} className="text-center text-muted-foreground">
                    Henüz platform eklenmemiş
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </CardContent>

      {/* Edit Dialog */}
      {editingItem && (
        <Dialog open={!!editingItem} onOpenChange={(open) => !open && setEditingItem(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Oyun Platformu Düzenle</DialogTitle>
              <DialogDescription>Platform bilgilerini güncelleyin</DialogDescription>
            </DialogHeader>
            <form onSubmit={onSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="edit-game-platform-name">Platform Adı</Label>
                <Input
                  id="edit-game-platform-name"
                  {...formMethods.register('name')}
                />
                {formMethods.formState.errors.name && (
                  <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                )}
              </div>
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setEditingItem(null)}>
                  İptal
                </Button>
                <Button type="submit" disabled={updateMutation.isPending}>
                  {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      )}

      {/* Delete Dialog */}
      {itemToDelete && (
        <Dialog open={!!itemToDelete} onOpenChange={(open) => !open && setItemToDelete(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Oyun Platformu Sil</DialogTitle>
              <DialogDescription>
                "{itemToDelete.name}" platformunu silmek istediğinize emin misiniz?
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setItemToDelete(null)}>
                İptal
              </Button>
              <Button
                type="button"
                variant="destructive"
                onClick={() => {
                  deleteMutation.mutate(itemToDelete.id);
                }}
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
}

// WatchPlatform Bölümü
function WatchPlatformSection() {
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<WatchPlatform | null>(null);
  const [itemToDelete, setItemToDelete] = useState<WatchPlatform | null>(null);

  const { data: platforms, isLoading } = useQuery({
    queryKey: ['watch-platforms'],
    queryFn: getAllWatchPlatforms
  });

  const formMethods = useForm<NameFormSchema>({
    resolver: zodResolver(nameSchema),
    defaultValues: { name: '' }
  });

  useEffect(() => {
    if (editingItem) {
      formMethods.reset({ name: editingItem.name });
    } else {
      formMethods.reset({ name: '' });
    }
  }, [editingItem, formMethods]);

  const createMutation = useMutation({
    mutationFn: (values: WatchPlatformFormValues) => createWatchPlatform(values),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İzleme platformu eklenemedi');
        return;
      }
      toast.success(result.message || 'İzleme platformu eklendi');
      setIsCreateOpen(false);
      queryClient.invalidateQueries({ queryKey: ['watch-platforms'] });
    },
    onError: (error) => handleApiError(error, 'İzleme platformu eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: WatchPlatformFormValues) =>
      editingItem ? updateWatchPlatform(editingItem.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İzleme platformu güncellenemedi');
        return;
      }
      toast.success(result.message || 'İzleme platformu güncellendi');
      setEditingItem(null);
      queryClient.invalidateQueries({ queryKey: ['watch-platforms'] });
    },
    onError: (error) => handleApiError(error, 'İzleme platformu güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteWatchPlatform(id),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İzleme platformu silinemedi');
        return;
      }
      toast.success(result.message || 'İzleme platformu silindi');
      setItemToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['watch-platforms'] });
    },
    onError: (error) => handleApiError(error, 'İzleme platformu silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    if (editingItem) {
      await updateMutation.mutateAsync({ name: values.name });
    } else {
      await createMutation.mutateAsync({ name: values.name });
    }
  });

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>İzleme Platformları</CardTitle>
          <Dialog open={isCreateOpen && !editingItem} onOpenChange={(open) => {
            setIsCreateOpen(open);
            if (open) setEditingItem(null);
          }}>
            <DialogTrigger asChild>
              <Button size="sm" onClick={() => setEditingItem(null)}>
                <PlusCircle className="h-4 w-4 mr-2" />
                Ekle
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Yeni İzleme Platformu Ekle</DialogTitle>
                <DialogDescription>İzleme platformu bilgilerini girin</DialogDescription>
              </DialogHeader>
              <form onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="watch-platform-name">Platform Adı</Label>
                  <Input
                    id="watch-platform-name"
                    {...formMethods.register('name')}
                    placeholder="Örn: Netflix, Prime, Disney"
                  />
                  {formMethods.formState.errors.name && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                  )}
                </div>
                <DialogFooter>
                  <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>
                    İptal
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="text-center py-4">Yükleniyor...</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ad</TableHead>
                <TableHead className="text-right">İşlemler</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {platforms && platforms.length > 0 ? (
                platforms.map((platform) => (
                  <TableRow key={platform.id}>
                    <TableCell>{platform.name}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setIsCreateOpen(false);
                            setEditingItem(platform);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setItemToDelete(platform)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={2} className="text-center text-muted-foreground">
                    Henüz platform eklenmemiş
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </CardContent>

      {/* Edit Dialog */}
      {editingItem && (
        <Dialog open={!!editingItem} onOpenChange={(open) => !open && setEditingItem(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>İzleme Platformu Düzenle</DialogTitle>
              <DialogDescription>Platform bilgilerini güncelleyin</DialogDescription>
            </DialogHeader>
            <form onSubmit={onSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="edit-watch-platform-name">Platform Adı</Label>
                <Input
                  id="edit-watch-platform-name"
                  {...formMethods.register('name')}
                />
                {formMethods.formState.errors.name && (
                  <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                )}
              </div>
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setEditingItem(null)}>
                  İptal
                </Button>
                <Button type="submit" disabled={updateMutation.isPending}>
                  {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      )}

      {/* Delete Dialog */}
      {itemToDelete && (
        <Dialog open={!!itemToDelete} onOpenChange={(open) => !open && setItemToDelete(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>İzleme Platformu Sil</DialogTitle>
              <DialogDescription>
                "{itemToDelete.name}" platformunu silmek istediğinize emin misiniz?
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setItemToDelete(null)}>
                İptal
              </Button>
              <Button
                type="button"
                variant="destructive"
                onClick={() => {
                  deleteMutation.mutate(itemToDelete.id);
                }}
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
}

// MovieSeriesGenre Bölümü
function MovieSeriesGenreSection() {
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<MovieSeriesGenre | null>(null);
  const [itemToDelete, setItemToDelete] = useState<MovieSeriesGenre | null>(null);

  const { data: genres, isLoading } = useQuery({
    queryKey: ['movie-series-genres'],
    queryFn: getAllMovieSeriesGenres
  });

  const formMethods = useForm<NameFormSchema>({
    resolver: zodResolver(nameSchema),
    defaultValues: { name: '' }
  });

  useEffect(() => {
    if (editingItem) {
      formMethods.reset({ name: editingItem.name });
    } else {
      formMethods.reset({ name: '' });
    }
  }, [editingItem, formMethods]);

  const createMutation = useMutation({
    mutationFn: (values: MovieSeriesGenreFormValues) => createMovieSeriesGenre(values),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi türü eklenemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi türü eklendi');
      setIsCreateOpen(false);
      queryClient.invalidateQueries({ queryKey: ['movie-series-genres'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi türü eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: MovieSeriesGenreFormValues) =>
      editingItem ? updateMovieSeriesGenre(editingItem.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi türü güncellenemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi türü güncellendi');
      setEditingItem(null);
      queryClient.invalidateQueries({ queryKey: ['movie-series-genres'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi türü güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteMovieSeriesGenre(id),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Film/Dizi türü silinemedi');
        return;
      }
      toast.success(result.message || 'Film/Dizi türü silindi');
      setItemToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['movie-series-genres'] });
    },
    onError: (error) => handleApiError(error, 'Film/Dizi türü silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    if (editingItem) {
      await updateMutation.mutateAsync({ name: values.name });
    } else {
      await createMutation.mutateAsync({ name: values.name });
    }
  });

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Film/Dizi Türleri</CardTitle>
          <Dialog open={isCreateOpen && !editingItem} onOpenChange={(open) => {
            setIsCreateOpen(open);
            if (open) setEditingItem(null);
          }}>
            <DialogTrigger asChild>
              <Button size="sm" onClick={() => setEditingItem(null)}>
                <PlusCircle className="h-4 w-4 mr-2" />
                Ekle
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Yeni Film/Dizi Türü Ekle</DialogTitle>
                <DialogDescription>Film/Dizi türü bilgilerini girin</DialogDescription>
              </DialogHeader>
              <form onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="genre-name">Tür Adı</Label>
                  <Input
                    id="genre-name"
                    {...formMethods.register('name')}
                    placeholder="Örn: Film, Dizi"
                  />
                  {formMethods.formState.errors.name && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                  )}
                </div>
                <DialogFooter>
                  <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>
                    İptal
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="text-center py-4">Yükleniyor...</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ad</TableHead>
                <TableHead className="text-right">İşlemler</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {genres && genres.length > 0 ? (
                genres.map((genre) => (
                  <TableRow key={genre.id}>
                    <TableCell>{genre.name}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setIsCreateOpen(false);
                            setEditingItem(genre);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setItemToDelete(genre)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={2} className="text-center text-muted-foreground">
                    Henüz tür eklenmemiş
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </CardContent>

      {/* Edit Dialog */}
      {editingItem && (
        <Dialog open={!!editingItem} onOpenChange={(open) => !open && setEditingItem(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Film/Dizi Türü Düzenle</DialogTitle>
              <DialogDescription>Tür bilgilerini güncelleyin</DialogDescription>
            </DialogHeader>
            <form onSubmit={onSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="edit-genre-name">Tür Adı</Label>
                <Input
                  id="edit-genre-name"
                  {...formMethods.register('name')}
                />
                {formMethods.formState.errors.name && (
                  <p className="text-sm text-destructive">{formMethods.formState.errors.name.message}</p>
                )}
              </div>
              <DialogFooter>
                <Button type="button" variant="outline" onClick={() => setEditingItem(null)}>
                  İptal
                </Button>
                <Button type="submit" disabled={updateMutation.isPending}>
                  {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      )}

      {/* Delete Dialog */}
      {itemToDelete && (
        <Dialog open={!!itemToDelete} onOpenChange={(open) => !open && setItemToDelete(null)}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Film/Dizi Türü Sil</DialogTitle>
              <DialogDescription>
                "{itemToDelete.name}" türünü silmek istediğinize emin misiniz?
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setItemToDelete(null)}>
                İptal
              </Button>
              <Button
                type="button"
                variant="destructive"
                onClick={() => {
                  deleteMutation.mutate(itemToDelete.id);
                }}
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
}

// Ana Sayfa Komponenti
export function ReferenceDataPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Referans Veriler</h1>
        <p className="text-muted-foreground">
          Oyun platformları, izleme platformları ve film/dizi türlerini yönetin
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1">
          <GamePlatformSection />
        </div>
        <div className="lg:col-span-1">
          <WatchPlatformSection />
        </div>
        <div className="lg:col-span-1">
          <MovieSeriesGenreSection />
        </div>
      </div>
    </div>
  );
}

