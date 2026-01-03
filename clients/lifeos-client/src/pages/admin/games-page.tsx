import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, Gamepad2 } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createGame, updateGame, deleteGame, fetchGames } from '../../features/games/api';
import { Game, GameFormValues, GameStatus, GameStatusLabels } from '../../features/games/types';
import { getAllGamePlatforms } from '../../features/gameplatforms/api';
import { getAllGameStores } from '../../features/gamestores/api';
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

const gameSchema = z.object({
  title: z.string().min(2, 'Oyun adı en az 2 karakter olmalıdır').max(200, 'Oyun adı en fazla 200 karakter olabilir'),
  coverUrl: z.string().url('Geçerli bir URL girin').optional().or(z.literal('')),
  gamePlatformId: z.string().uuid('Geçerli bir platform seçin'),
  gameStoreId: z.string().uuid('Geçerli bir mağaza seçin'),
  status: z.nativeEnum(GameStatus),
  isOwned: z.boolean()
});

type GameFormSchema = z.infer<typeof gameSchema>;

export function GamesPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize] = useState(10);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingGame, setEditingGame] = useState<Game | null>(null);
  const [gameToDelete, setGameToDelete] = useState<Game | null>(null);
  const [selectedPlatform, setSelectedPlatform] = useState<string | 'all'>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['games', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchGames({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'createdDate', dir: 'desc' }
    })
  });

  const { data: gamePlatforms } = useQuery({
    queryKey: ['game-platforms'],
    queryFn: getAllGamePlatforms
  });

  const { data: gameStores } = useQuery({
    queryKey: ['game-stores'],
    queryFn: getAllGameStores
  });

  const formMethods = useForm<GameFormSchema>({
    resolver: zodResolver(gameSchema),
    defaultValues: {
      title: '',
      coverUrl: '',
      gamePlatformId: '',
      gameStoreId: '',
      status: GameStatus.Backlog,
      isOwned: false
    }
  });

  useEffect(() => {
    if (editingGame) {
      formMethods.reset({
        title: editingGame.title,
        coverUrl: editingGame.coverUrl || '',
        gamePlatformId: editingGame.gamePlatformId,
        gameStoreId: editingGame.gameStoreId,
        status: editingGame.status,
        isOwned: editingGame.isOwned
      });
    } else if (isCreateOpen) {
      // Dialog açıldığında formu reset et
      formMethods.reset({
        title: '',
        coverUrl: '',
        gamePlatformId: gamePlatforms?.[0]?.id || '',
        gameStoreId: gameStores?.[0]?.id || '',
        status: GameStatus.Backlog,
        isOwned: false
      });
    }
  }, [editingGame, isCreateOpen, formMethods, gamePlatforms, gameStores]);

  const createMutation = useMutation({
    mutationFn: createGame,
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun eklenemedi');
        return;
      }
      toast.success(result.message || 'Oyun eklendi');
      setIsCreateOpen(false);
      // Formu reset et
      formMethods.reset({
        title: '',
        coverUrl: '',
        gamePlatformId: gamePlatforms?.[0]?.id || '',
        gameStoreId: gameStores?.[0]?.id || '',
        status: GameStatus.Backlog,
        isOwned: false
      });
      queryClient.invalidateQueries({ queryKey: ['games'] });
    },
    onError: (error) => handleApiError(error, 'Oyun eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: GameFormValues) =>
      editingGame ? updateGame(editingGame.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun güncellenemedi');
        return;
      }
      toast.success(result.message || 'Oyun güncellendi');
      setEditingGame(null);
      queryClient.invalidateQueries({ queryKey: ['games'] });
    },
    onError: (error) => handleApiError(error, 'Oyun güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (gameId: string) => deleteGame(gameId),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Oyun silinemedi');
        return;
      }
      toast.success(result.message || 'Oyun silindi');
      setGameToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['games'] });
    },
    onError: (error) => handleApiError(error, 'Oyun silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    const formData: GameFormValues = {
      title: values.title,
      coverUrl: values.coverUrl || undefined,
      gamePlatformId: values.gamePlatformId,
      gameStoreId: values.gameStoreId,
      status: values.status,
      isOwned: values.isOwned
    };
    if (editingGame) {
      await updateMutation.mutateAsync(formData);
    } else {
      await createMutation.mutateAsync(formData);
    }
  });

  const filteredGames = useMemo(() => {
    if (!data?.items) return [];
    if (selectedPlatform === 'all') return data.items;
    return data.items.filter(game => game.gamePlatformId === selectedPlatform);
  }, [data, selectedPlatform]);

  const platforms = [
    { value: 'all' as const, label: 'Tümü' },
    ...(gamePlatforms || []).map(p => ({ value: p.id, label: p.name }))
  ];

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle className="text-lg sm:text-2xl">Oyun Kütüphanesi</CardTitle>
            <p className="text-xs sm:text-sm text-muted-foreground">
              Oyunlarınızı platform bazında organize edin ve takip edin.
            </p>
          </div>
          <Dialog
            open={isCreateOpen}
            onOpenChange={(open) => {
              setIsCreateOpen(open);
              if (open) {
                // Dialog açıldığında formu reset et
                formMethods.reset({
                  title: '',
                  coverUrl: '',
                  gamePlatformId: gamePlatforms?.[0]?.id || '',
                  gameStoreId: gameStores?.[0]?.id || '',
                  status: GameStatus.Backlog,
                  isOwned: false
                });
              }
            }}
          >
            <DialogTrigger asChild>
              <Button className="gap-2 text-xs sm:text-sm">
                <PlusCircle className="h-3.5 w-3.5 sm:h-4 sm:w-4" /> <span className="hidden sm:inline">Yeni Oyun</span><span className="sm:hidden">Ekle</span>
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-full sm:max-w-lg max-h-[90vh] overflow-y-auto mx-4">
              <DialogHeader>
                <DialogTitle>Yeni Oyun</DialogTitle>
                <DialogDescription>Oyun kütüphanenize yeni bir oyun ekleyin.</DialogDescription>
              </DialogHeader>
              <form id="create-game-form" onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="game-title">Oyun Adı *</Label>
                  <Input id="game-title" placeholder="Oyun adı" {...formMethods.register('title')} />
                  {formMethods.formState.errors.title && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.title.message}</p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="game-cover">Kapak URL</Label>
                  <Input id="game-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="game-platform">Platform</Label>
                    <select
                      id="game-platform"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('gamePlatformId')}
                    >
                      {gamePlatforms?.map(platform => (
                        <option key={platform.id} value={platform.id}>
                          {platform.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="game-store">Mağaza</Label>
                    <select
                      id="game-store"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('gameStoreId')}
                    >
                      {gameStores?.map(store => (
                        <option key={store.id} value={store.id}>
                          {store.name}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="game-status">Durum</Label>
                    <select
                      id="game-status"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('status', { valueAsNumber: true })}
                    >
                      {Object.entries(GameStatusLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="game-owned">Sahip Olunan</Label>
                    <div className="flex items-center gap-2 pt-2">
                      <input
                        id="game-owned"
                        type="checkbox"
                        {...formMethods.register('isOwned')}
                        className="h-4 w-4 rounded border-gray-300"
                      />
                      <Label htmlFor="game-owned" className="cursor-pointer">
                        Bu oyuna sahibim
                      </Label>
                    </div>
                  </div>
                </div>
              </form>
              <DialogFooter className="flex-col sm:flex-row gap-2">
                <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)} className="w-full sm:w-auto">
                  İptal
                </Button>
                <Button type="submit" form="create-game-form" disabled={createMutation.isPending} className="w-full sm:w-auto">
                  {createMutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="flex flex-wrap gap-2">
              {platforms.map((platform) => (
                <Button
                  key={platform.value}
                  variant={selectedPlatform === platform.value ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setSelectedPlatform(platform.value)}
                >
                  {platform.label}
                </Button>
              ))}
            </div>
            <Input
              placeholder="Oyun ara..."
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
              {filteredGames.map((game) => (
                <Card key={game.id} className="relative flex flex-col overflow-hidden group">
                  {/* Resim üstte, dikey format */}
                  {game.coverUrl && (
                    <div className="relative w-full aspect-[3/4] overflow-hidden bg-muted">
                      <img
                        src={game.coverUrl}
                        alt={game.title}
                        className="w-full h-full object-cover transition-transform group-hover:scale-105"
                      />
                      {/* Action buttons overlay */}
                      <div className="absolute top-1.5 right-1.5 flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button
                          variant="secondary"
                          size="icon"
                          className="h-6 w-6 bg-background/80 backdrop-blur-sm hover:bg-background"
                          onClick={() => setEditingGame(game)}
                        >
                          <Pencil className="h-3 w-3" />
                        </Button>
                        <Button
                          variant="secondary"
                          size="icon"
                          className="h-6 w-6 bg-background/80 backdrop-blur-sm hover:bg-destructive hover:text-destructive-foreground"
                          onClick={() => setGameToDelete(game)}
                        >
                          <Trash2 className="h-3 w-3" />
                        </Button>
                      </div>
                    </div>
                  )}
                  {/* İçerik */}
                  <CardContent className="p-2 sm:p-3 space-y-1.5 flex-1 flex flex-col">
                    {!game.coverUrl && (
                      <div className="flex items-start justify-between gap-1.5 mb-1">
                        <div className="flex-1 min-w-0">
                          <CardTitle className="text-xs sm:text-sm font-semibold line-clamp-2 leading-tight">{game.title}</CardTitle>
                        </div>
                        <div className="flex gap-0.5 shrink-0">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 sm:h-7 sm:w-7"
                            onClick={() => setEditingGame(game)}
                          >
                            <Pencil className="h-3 w-3 sm:h-3.5 sm:w-3.5" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 sm:h-7 sm:w-7"
                            onClick={() => setGameToDelete(game)}
                          >
                            <Trash2 className="h-3 w-3 sm:h-3.5 sm:w-3.5 text-destructive" />
                          </Button>
                        </div>
                      </div>
                    )}
                    {game.coverUrl && (
                      <div className="min-w-0">
                        <CardTitle className="text-xs sm:text-sm font-semibold line-clamp-2 leading-tight">{game.title}</CardTitle>
                      </div>
                    )}
                    <div className="flex items-center gap-1 flex-wrap">
                      <Badge variant="secondary" className="text-[10px] px-1 py-0">{game.gamePlatformName}</Badge>
                      <Badge variant="outline" className="text-[10px] px-1 py-0">{GameStatusLabels[game.status]}</Badge>
                      {game.isOwned && <Badge variant="default" className="text-[10px] px-1 py-0">Sahip</Badge>}
                    </div>
                    <div className="flex items-center gap-1 text-[10px] sm:text-xs text-muted-foreground pt-0.5">
                      <Gamepad2 className="h-2.5 w-2.5 sm:h-3 sm:w-3 shrink-0" />
                      <span className="truncate">{game.gameStoreName}</span>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
          {filteredGames.length === 0 && !isLoading && (
            <div className="text-center py-8 text-muted-foreground">
              Oyun bulunamadı.
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingGame} onOpenChange={(open) => !open && setEditingGame(null)}>
        <DialogContent className="max-w-full sm:max-w-lg max-h-[90vh] overflow-y-auto mx-4">
          <DialogHeader>
            <DialogTitle>Oyunu Düzenle</DialogTitle>
            <DialogDescription>Oyun bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-game-form" onSubmit={onSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="edit-game-title">Oyun Adı *</Label>
              <Input id="edit-game-title" {...formMethods.register('title')} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-game-cover">Kapak URL</Label>
              <Input id="edit-game-cover" {...formMethods.register('coverUrl')} />
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-game-platform">Platform</Label>
                <select
                  id="edit-game-platform"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('gamePlatformId')}
                >
                  {gamePlatforms?.map(platform => (
                    <option key={platform.id} value={platform.id}>
                      {platform.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-game-store">Mağaza</Label>
                <select
                  id="edit-game-store"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('gameStoreId')}
                >
                  {gameStores?.map(store => (
                    <option key={store.id} value={store.id}>
                      {store.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-game-status">Durum</Label>
                <select
                  id="edit-game-status"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('status', { valueAsNumber: true })}
                >
                  {Object.entries(GameStatusLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-game-owned">Sahip Olunan</Label>
                <div className="flex items-center gap-2 pt-2">
                  <input
                    id="edit-game-owned"
                    type="checkbox"
                    {...formMethods.register('isOwned')}
                    className="h-4 w-4 rounded border-gray-300"
                  />
                  <Label htmlFor="edit-game-owned" className="cursor-pointer">
                    Bu oyuna sahibim
                  </Label>
                </div>
              </div>
            </div>
          </form>
          <DialogFooter className="flex-col sm:flex-row gap-2">
            <Button type="button" variant="ghost" onClick={() => setEditingGame(null)} className="w-full sm:w-auto">
              İptal
            </Button>
            <Button type="submit" form="edit-game-form" disabled={updateMutation.isPending} className="w-full sm:w-auto">
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!gameToDelete} onOpenChange={(open) => !open && setGameToDelete(null)}>
        <DialogContent className="max-w-full sm:max-w-md mx-4">
          <DialogHeader>
            <DialogTitle>Oyunu Sil</DialogTitle>
            <DialogDescription>
              Bu oyunu silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek oyun: <span className="font-medium">{gameToDelete?.title}</span>
          </p>
          <DialogFooter className="flex-col sm:flex-row gap-2">
            <Button type="button" variant="ghost" onClick={() => setGameToDelete(null)} className="w-full sm:w-auto">
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => gameToDelete && deleteMutation.mutate(gameToDelete.id)}
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

