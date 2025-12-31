import { useState, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2 } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { getAllGamePlatforms, createGamePlatform, updateGamePlatform, deleteGamePlatform } from '../../features/gameplatforms/api';
import { GamePlatform, GamePlatformFormValues } from '../../features/gameplatforms/types';
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
import { PermissionGuard } from '../../components/auth/permission-guard';
import { Permissions } from '../../lib/permissions';

const nameSchema = z.object({
  name: z.string().min(1, 'Ad boş olamaz').max(100, 'Ad en fazla 100 karakter olabilir')
});

type NameFormSchema = z.infer<typeof nameSchema>;

export function GamePlatformsPage() {
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
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Oyun Platformları</h1>
          <p className="text-muted-foreground mt-2">Oyun platformlarını yönetin</p>
        </div>
        <PermissionGuard requiredPermission={Permissions.GamePlatformsCreate}>
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
        </PermissionGuard>
      </div>

      {/* Table */}
      <Card>
        <CardContent className="pt-6">
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
                          <PermissionGuard requiredPermission={Permissions.GamePlatformsUpdate}>
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
                          </PermissionGuard>
                          <PermissionGuard requiredPermission={Permissions.GamePlatformsDelete}>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => setItemToDelete(platform)}
                            >
                              <Trash2 className="h-4 w-4 text-destructive" />
                            </Button>
                          </PermissionGuard>
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
      </Card>

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
    </div>
  );
}

