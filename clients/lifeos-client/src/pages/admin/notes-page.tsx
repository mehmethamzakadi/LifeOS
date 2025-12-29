import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, Pin, Tag } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createPersonalNote, updatePersonalNote, deletePersonalNote, fetchPersonalNotes } from '../../features/personalnotes/api';
import { PersonalNote, PersonalNoteFormValues } from '../../features/personalnotes/types';
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
import { cn } from '../../lib/utils';
import { RichTextEditor } from '../../components/editor/rich-text-editor';

const noteSchema = z.object({
  title: z.string().min(2, 'Başlık en az 2 karakter olmalıdır').max(200, 'Başlık en fazla 200 karakter olabilir'),
  content: z.string().min(1, 'İçerik boş olamaz'),
  category: z.string().max(100).optional().or(z.literal('')),
  isPinned: z.boolean(),
  tags: z.string().max(500).optional().or(z.literal(''))
});

type NoteFormSchema = z.infer<typeof noteSchema>;

export function NotesPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize] = useState(20);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingNote, setEditingNote] = useState<PersonalNote | null>(null);
  const [noteToDelete, setNoteToDelete] = useState<PersonalNote | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['personalnotes', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchPersonalNotes({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'createdDate', dir: 'desc' }
    })
  });

  const formMethods = useForm<NoteFormSchema>({
    resolver: zodResolver(noteSchema),
    defaultValues: {
      title: '',
      content: '',
      category: '',
      isPinned: false,
      tags: ''
    }
  });

  useEffect(() => {
    if (editingNote) {
      formMethods.reset({
        title: editingNote.title,
        content: editingNote.content,
        category: editingNote.category || '',
        isPinned: editingNote.isPinned,
        tags: editingNote.tags || ''
      });
    } else {
      formMethods.reset({
        title: '',
        content: '',
        category: '',
        isPinned: false,
        tags: ''
      });
    }
  }, [editingNote, formMethods]);

  const createMutation = useMutation({
    mutationFn: createPersonalNote,
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Not eklenemedi');
        return;
      }
      toast.success(result.message || 'Not eklendi');
      setIsCreateOpen(false);
      formMethods.reset({
        title: '',
        content: '',
        category: '',
        isPinned: false,
        tags: ''
      });
      queryClient.invalidateQueries({ queryKey: ['personalnotes'] });
    },
    onError: (error) => handleApiError(error, 'Not eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: PersonalNoteFormValues) =>
      editingNote ? updatePersonalNote(editingNote.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Not güncellenemedi');
        return;
      }
      toast.success(result.message || 'Not güncellendi');
      setEditingNote(null);
      queryClient.invalidateQueries({ queryKey: ['personalnotes'] });
    },
    onError: (error) => handleApiError(error, 'Not güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (noteId: string) => deletePersonalNote(noteId),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Not silinemedi');
        return;
      }
      toast.success(result.message || 'Not silindi');
      setNoteToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['personalnotes'] });
    },
    onError: (error) => handleApiError(error, 'Not silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    const formData: PersonalNoteFormValues = {
      title: values.title,
      content: values.content,
      category: values.category || undefined,
      isPinned: values.isPinned,
      tags: values.tags || undefined
    };
    if (editingNote) {
      await updateMutation.mutateAsync(formData);
    } else {
      await createMutation.mutateAsync(formData);
    }
  });

  const categories = useMemo(() => {
    if (!data?.items) return [];
    const cats = new Set<string>();
    data.items.forEach(note => {
      if (note.category) cats.add(note.category);
    });
    return Array.from(cats).sort();
  }, [data]);

  const filteredNotes = useMemo(() => {
    if (!data?.items) return [];
    let filtered = data.items;
    if (selectedCategory !== 'all') {
      filtered = filtered.filter(note => note.category === selectedCategory);
    }
    // Pinned notes first
    return filtered.sort((a, b) => {
      if (a.isPinned && !b.isPinned) return -1;
      if (!a.isPinned && b.isPinned) return 1;
      return 0;
    });
  }, [data, selectedCategory]);

  const parseTags = (tags?: string) => {
    if (!tags) return [];
    return tags.split(',').map(t => t.trim()).filter(t => t.length > 0);
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle>Kişisel Notlar</CardTitle>
            <p className="text-sm text-muted-foreground">
              Notlarınızı organize edin ve yönetin. Pinterest tarzı masonry layout.
            </p>
          </div>
          <Dialog
            open={isCreateOpen}
            onOpenChange={(open) => {
              setIsCreateOpen(open);
              if (!open && !editingNote) {
                formMethods.reset({
                  title: '',
                  content: '',
                  category: '',
                  isPinned: false,
                  tags: ''
                });
              }
            }}
          >
            <DialogTrigger asChild>
              <Button className="gap-2">
                <PlusCircle className="h-4 w-4" /> Yeni Not
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Yeni Not</DialogTitle>
                <DialogDescription>Yeni bir kişisel not oluşturun.</DialogDescription>
              </DialogHeader>
              <form id="create-note-form" onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="note-title">Başlık *</Label>
                  <Input id="note-title" placeholder="Not başlığı" {...formMethods.register('title')} />
                  {formMethods.formState.errors.title && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.title.message}</p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label htmlFor="note-content">İçerik *</Label>
                  <RichTextEditor
                    value={formMethods.watch('content')}
                    onChange={(value) => formMethods.setValue('content', value)}
                  />
                  {formMethods.formState.errors.content && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.content.message}</p>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="note-category">Kategori</Label>
                    <Input id="note-category" placeholder="Kategori" {...formMethods.register('category')} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="note-tags">Etiketler (virgülle ayırın)</Label>
                    <Input id="note-tags" placeholder="etiket1, etiket2" {...formMethods.register('tags')} />
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <input
                    id="note-pinned"
                    type="checkbox"
                    {...formMethods.register('isPinned')}
                    className="h-4 w-4 rounded border-gray-300"
                  />
                  <Label htmlFor="note-pinned" className="cursor-pointer">
                    Sabitle
                  </Label>
                </div>
              </form>
              <DialogFooter>
                <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)}>
                  İptal
                </Button>
                <Button type="submit" form="create-note-form" disabled={createMutation.isPending}>
                  {createMutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="flex flex-wrap gap-2">
              <Button
                variant={selectedCategory === 'all' ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedCategory('all')}
              >
                Tümü
              </Button>
              {categories.map((cat) => (
                <Button
                  key={cat}
                  variant={selectedCategory === cat ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setSelectedCategory(cat)}
                >
                  {cat}
                </Button>
              ))}
            </div>
            <Input
              placeholder="Not ara..."
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
            <div className="columns-1 md:columns-2 lg:columns-3 gap-4 space-y-4">
              {filteredNotes.map((note) => {
                const tags = parseTags(note.tags);
                return (
                  <Card
                    key={note.id}
                    className={cn(
                      'break-inside-avoid mb-4 relative',
                      note.isPinned && 'ring-2 ring-primary'
                    )}
                  >
                    <CardHeader className="pb-3">
                      <div className="flex items-start justify-between gap-2">
                        <CardTitle className="text-base leading-tight">{note.title}</CardTitle>
                        {note.isPinned && (
                          <Pin className="h-4 w-4 text-primary shrink-0" fill="currentColor" />
                        )}
                      </div>
                      {note.category && (
                        <Badge variant="secondary" className="mt-2">
                          {note.category}
                        </Badge>
                      )}
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <div
                        className="text-sm prose prose-sm max-w-none dark:prose-invert"
                        dangerouslySetInnerHTML={{ __html: note.content }}
                      />
                      {tags.length > 0 && (
                        <div className="flex flex-wrap gap-1">
                          {tags.map((tag, idx) => (
                            <Badge key={idx} variant="outline" className="text-xs">
                              <Tag className="h-3 w-3 mr-1" />
                              {tag}
                            </Badge>
                          ))}
                        </div>
                      )}
                      <div className="flex items-center justify-between pt-2 border-t">
                        <span className="text-xs text-muted-foreground">
                          {new Date(note.createdDate).toLocaleDateString('tr-TR')}
                        </span>
                        <div className="flex gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7"
                            onClick={() => setEditingNote(note)}
                          >
                            <Pencil className="h-3 w-3" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7"
                            onClick={() => setNoteToDelete(note)}
                          >
                            <Trash2 className="h-3 w-3 text-destructive" />
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
          {filteredNotes.length === 0 && !isLoading && (
            <div className="text-center py-8 text-muted-foreground">
              Not bulunamadı.
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingNote} onOpenChange={(open) => !open && setEditingNote(null)}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Notu Düzenle</DialogTitle>
            <DialogDescription>Not bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-note-form" onSubmit={onSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="edit-note-title">Başlık *</Label>
              <Input id="edit-note-title" {...formMethods.register('title')} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-note-content">İçerik *</Label>
              <RichTextEditor
                value={formMethods.watch('content')}
                onChange={(value) => formMethods.setValue('content', value)}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-note-category">Kategori</Label>
                <Input id="edit-note-category" {...formMethods.register('category')} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-note-tags">Etiketler</Label>
                <Input id="edit-note-tags" {...formMethods.register('tags')} />
              </div>
            </div>
            <div className="flex items-center gap-2">
              <input
                id="edit-note-pinned"
                type="checkbox"
                {...formMethods.register('isPinned')}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Label htmlFor="edit-note-pinned" className="cursor-pointer">
                Sabitle
              </Label>
            </div>
          </form>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setEditingNote(null)}>
              İptal
            </Button>
            <Button type="submit" form="edit-note-form" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!noteToDelete} onOpenChange={(open) => !open && setNoteToDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Notu Sil</DialogTitle>
            <DialogDescription>
              Bu notu silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek not: <span className="font-medium">{noteToDelete?.title}</span>
          </p>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setNoteToDelete(null)}>
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => noteToDelete && deleteMutation.mutate(noteToDelete.id)}
            >
              {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

