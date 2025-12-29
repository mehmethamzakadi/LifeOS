import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, BookOpen, BookMarked } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createBook, updateBook, deleteBook, fetchBooks } from '../../features/books/api';
import { Book, BookFormValues, BookStatus, BookStatusLabels } from '../../features/books/types';
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

const bookSchema = z.object({
  title: z.string().min(2, 'Kitap adı en az 2 karakter olmalıdır').max(200, 'Kitap adı en fazla 200 karakter olabilir'),
  author: z.string().min(2, 'Yazar adı en az 2 karakter olmalıdır').max(100, 'Yazar adı en fazla 100 karakter olabilir'),
  coverUrl: z.string().url('Geçerli bir URL girin').optional().or(z.literal('')),
  totalPages: z.number().min(0, 'Toplam sayfa sayısı 0 veya daha büyük olmalıdır'),
  currentPage: z.number().min(0, 'Mevcut sayfa sayısı 0 veya daha büyük olmalıdır'),
  status: z.nativeEnum(BookStatus),
  rating: z.number().min(1).max(10).optional().or(z.literal('')),
  startDate: z.string().optional().or(z.literal('')),
  endDate: z.string().optional().or(z.literal(''))
}).refine((data) => data.currentPage <= data.totalPages, {
  message: 'Mevcut sayfa sayısı toplam sayfa sayısından büyük olamaz',
  path: ['currentPage']
});

type BookFormSchema = z.infer<typeof bookSchema>;

export function BooksPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize] = useState(10);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingBook, setEditingBook] = useState<Book | null>(null);
  const [bookToDelete, setBookToDelete] = useState<Book | null>(null);
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['books', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchBooks({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'createdDate', dir: 'desc' }
    })
  });

  const formMethods = useForm<BookFormSchema>({
    resolver: zodResolver(bookSchema),
    defaultValues: {
      title: '',
      author: '',
      coverUrl: '',
      totalPages: 0,
      currentPage: 0,
      status: BookStatus.ToRead,
      rating: undefined,
      startDate: '',
      endDate: ''
    }
  });

  // Helper function to format date for HTML date input (YYYY-MM-DD)
  const formatDateForInput = (dateString?: string): string => {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '';
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    } catch {
      return '';
    }
  };

  useEffect(() => {
    if (editingBook) {
      formMethods.reset({
        title: editingBook.title,
        author: editingBook.author,
        coverUrl: editingBook.coverUrl || '',
        totalPages: editingBook.totalPages,
        currentPage: editingBook.currentPage,
        status: editingBook.status,
        rating: editingBook.rating,
        startDate: formatDateForInput(editingBook.startDate),
        endDate: formatDateForInput(editingBook.endDate)
      });
    } else {
      formMethods.reset({
        title: '',
        author: '',
        coverUrl: '',
        totalPages: 0,
        currentPage: 0,
        status: BookStatus.ToRead,
        rating: undefined,
        startDate: '',
        endDate: ''
      });
    }
  }, [editingBook, formMethods]);

  const createMutation = useMutation({
    mutationFn: createBook,
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Kitap eklenemedi');
        return;
      }
      toast.success(result.message || 'Kitap eklendi');
      setIsCreateOpen(false);
      queryClient.invalidateQueries({ queryKey: ['books'] });
    },
    onError: (error) => handleApiError(error, 'Kitap eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: BookFormValues) =>
      editingBook ? updateBook(editingBook.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Kitap güncellenemedi');
        return;
      }
      toast.success(result.message || 'Kitap güncellendi');
      setEditingBook(null);
      queryClient.invalidateQueries({ queryKey: ['books'] });
    },
    onError: (error) => handleApiError(error, 'Kitap güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (bookId: string) => deleteBook(bookId),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Kitap silinemedi');
        return;
      }
      toast.success(result.message || 'Kitap silindi');
      setBookToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['books'] });
    },
    onError: (error) => handleApiError(error, 'Kitap silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    const formData: BookFormValues = {
      title: values.title,
      author: values.author,
      coverUrl: values.coverUrl || undefined,
      totalPages: values.totalPages,
      currentPage: values.currentPage,
      status: values.status,
      rating: values.rating || undefined,
      startDate: values.startDate || undefined,
      endDate: values.endDate || undefined
    };
    if (editingBook) {
      await updateMutation.mutateAsync(formData);
    } else {
      await createMutation.mutateAsync(formData);
    }
  });

  const getProgressPercentage = (book: Book) => {
    if (book.totalPages === 0) return 0;
    return Math.round((book.currentPage / book.totalPages) * 100);
  };

  const filteredBooks = useMemo(() => {
    if (!data?.items) return [];
    return data.items;
  }, [data]);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle>Kitap Kütüphanesi</CardTitle>
            <p className="text-sm text-muted-foreground">
              Okuduğunuz kitapları takip edin ve ilerlemenizi görüntüleyin.
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant={viewMode === 'grid' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setViewMode('grid')}
            >
              Grid
            </Button>
            <Button
              variant={viewMode === 'table' ? 'default' : 'outline'}
              size="sm"
              onClick={() => setViewMode('table')}
            >
              Tablo
            </Button>
            <Dialog
              open={isCreateOpen}
              onOpenChange={(open) => {
                setIsCreateOpen(open);
                if (!open && !editingBook) {
                  formMethods.reset();
                }
              }}
            >
              <DialogTrigger asChild>
                <Button className="gap-2">
                  <PlusCircle className="h-4 w-4" /> Yeni Kitap
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                  <DialogTitle>Yeni Kitap</DialogTitle>
                  <DialogDescription>Okuduğunuz kitabı ekleyin.</DialogDescription>
                </DialogHeader>
                <form id="create-book-form" onSubmit={onSubmit} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="book-title">Kitap Adı *</Label>
                      <Input id="book-title" placeholder="Kitap adı" {...formMethods.register('title')} />
                      {formMethods.formState.errors.title && (
                        <p className="text-sm text-destructive">{formMethods.formState.errors.title.message}</p>
                      )}
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="book-author">Yazar *</Label>
                      <Input id="book-author" placeholder="Yazar adı" {...formMethods.register('author')} />
                      {formMethods.formState.errors.author && (
                        <p className="text-sm text-destructive">{formMethods.formState.errors.author.message}</p>
                      )}
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="book-cover">Kapak URL</Label>
                    <Input id="book-cover" placeholder="https://..." {...formMethods.register('coverUrl')} />
                    {formMethods.formState.errors.coverUrl && (
                      <p className="text-sm text-destructive">{formMethods.formState.errors.coverUrl.message}</p>
                    )}
                  </div>
                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="book-total-pages">Toplam Sayfa</Label>
                      <Input
                        id="book-total-pages"
                        type="number"
                        {...formMethods.register('totalPages', { valueAsNumber: true })}
                      />
                      {formMethods.formState.errors.totalPages && (
                        <p className="text-sm text-destructive">{formMethods.formState.errors.totalPages.message}</p>
                      )}
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="book-current-page">Mevcut Sayfa</Label>
                      <Input
                        id="book-current-page"
                        type="number"
                        {...formMethods.register('currentPage', { valueAsNumber: true })}
                      />
                      {formMethods.formState.errors.currentPage && (
                        <p className="text-sm text-destructive">{formMethods.formState.errors.currentPage.message}</p>
                      )}
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="book-rating">Değerlendirme (1-10)</Label>
                      <Input
                        id="book-rating"
                        type="number"
                        min="1"
                        max="10"
                        {...formMethods.register('rating', { valueAsNumber: true })}
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="book-status">Durum</Label>
                      <select
                        id="book-status"
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                        {...formMethods.register('status', { valueAsNumber: true })}
                      >
                        {Object.entries(BookStatusLabels).map(([key, label]) => (
                          <option key={key} value={key}>
                            {label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="book-start-date">Başlangıç Tarihi</Label>
                      <Input id="book-start-date" type="date" {...formMethods.register('startDate')} />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="book-end-date">Bitiş Tarihi</Label>
                    <Input id="book-end-date" type="date" {...formMethods.register('endDate')} />
                  </div>
                </form>
                <DialogFooter>
                  <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)}>
                    İptal
                  </Button>
                  <Button type="submit" form="create-book-form" disabled={createMutation.isPending}>
                    {createMutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input
            placeholder="Kitap ara..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPageIndex(0);
            }}
            className="max-w-sm"
          />
          {isLoading ? (
            <div className="text-center py-8">Yükleniyor...</div>
          ) : viewMode === 'grid' ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {filteredBooks.map((book) => {
                const progress = getProgressPercentage(book);
                return (
                  <Card key={book.id} className="relative">
                    <CardHeader>
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <CardTitle className="text-lg">{book.title}</CardTitle>
                          <p className="text-sm text-muted-foreground mt-1">{book.author}</p>
                        </div>
                        <div className="flex gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => setEditingBook(book)}
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => setBookToDelete(book)}
                          >
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      </div>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      {book.coverUrl && (
                        <img
                          src={book.coverUrl}
                          alt={book.title}
                          className="w-full h-48 object-cover rounded-md"
                        />
                      )}
                      <div className="space-y-2">
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-muted-foreground">İlerleme</span>
                          <span className="font-medium">{progress}%</span>
                        </div>
                        <div className="w-full bg-muted rounded-full h-2">
                          <div
                            className="bg-primary h-2 rounded-full transition-all"
                            style={{ width: `${progress}%` }}
                          />
                        </div>
                        <div className="flex items-center justify-between text-xs text-muted-foreground">
                          <span>{book.currentPage} / {book.totalPages} sayfa</span>
                          {book.rating && <span>⭐ {book.rating}/10</span>}
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge variant="secondary">{BookStatusLabels[book.status]}</Badge>
                        {book.status === BookStatus.Reading && (
                          <BookOpen className="h-4 w-4 text-primary" />
                        )}
                        {book.status === BookStatus.Completed && (
                          <BookMarked className="h-4 w-4 text-green-500" />
                        )}
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          ) : (
            <div className="space-y-4">
              {filteredBooks.map((book) => {
                const progress = getProgressPercentage(book);
                return (
                  <Card key={book.id}>
                    <CardContent className="p-4">
                      <div className="flex items-center justify-between">
                        <div className="flex-1 space-y-2">
                          <div className="flex items-center gap-3">
                            <h3 className="font-semibold">{book.title}</h3>
                            <Badge variant="secondary">{BookStatusLabels[book.status]}</Badge>
                          </div>
                          <p className="text-sm text-muted-foreground">{book.author}</p>
                          <div className="space-y-1">
                            <div className="flex items-center justify-between text-sm">
                              <span className="text-muted-foreground">İlerleme</span>
                              <span className="font-medium">{progress}%</span>
                            </div>
                            <div className="w-full bg-muted rounded-full h-2">
                              <div
                                className="bg-primary h-2 rounded-full transition-all"
                                style={{ width: `${progress}%` }}
                              />
                            </div>
                            <div className="flex items-center justify-between text-xs text-muted-foreground">
                              <span>{book.currentPage} / {book.totalPages} sayfa</span>
                              {book.rating && <span>⭐ {book.rating}/10</span>}
                            </div>
                          </div>
                        </div>
                        <div className="flex gap-2">
                          <Button variant="ghost" size="icon" onClick={() => setEditingBook(book)}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => setBookToDelete(book)}>
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
          {filteredBooks.length === 0 && !isLoading && (
            <div className="text-center py-8 text-muted-foreground">
              Kitap bulunamadı.
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingBook} onOpenChange={(open) => !open && setEditingBook(null)}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Kitabı Düzenle</DialogTitle>
            <DialogDescription>Kitap bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-book-form" onSubmit={onSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-book-title">Kitap Adı *</Label>
                <Input id="edit-book-title" {...formMethods.register('title')} />
                {formMethods.formState.errors.title && (
                  <p className="text-sm text-destructive">{formMethods.formState.errors.title.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-book-author">Yazar *</Label>
                <Input id="edit-book-author" {...formMethods.register('author')} />
                {formMethods.formState.errors.author && (
                  <p className="text-sm text-destructive">{formMethods.formState.errors.author.message}</p>
                )}
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-book-cover">Kapak URL</Label>
              <Input id="edit-book-cover" {...formMethods.register('coverUrl')} />
            </div>
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-book-total-pages">Toplam Sayfa</Label>
                <Input
                  id="edit-book-total-pages"
                  type="number"
                  {...formMethods.register('totalPages', { valueAsNumber: true })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-book-current-page">Mevcut Sayfa</Label>
                <Input
                  id="edit-book-current-page"
                  type="number"
                  {...formMethods.register('currentPage', { valueAsNumber: true })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-book-rating">Değerlendirme</Label>
                <Input
                  id="edit-book-rating"
                  type="number"
                  min="1"
                  max="10"
                  {...formMethods.register('rating', { valueAsNumber: true })}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-book-status">Durum</Label>
                <select
                  id="edit-book-status"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('status', { valueAsNumber: true })}
                >
                  {Object.entries(BookStatusLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-book-start-date">Başlangıç Tarihi</Label>
                <Input id="edit-book-start-date" type="date" {...formMethods.register('startDate')} />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-book-end-date">Bitiş Tarihi</Label>
              <Input id="edit-book-end-date" type="date" {...formMethods.register('endDate')} />
            </div>
          </form>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setEditingBook(null)}>
              İptal
            </Button>
            <Button type="submit" form="edit-book-form" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!bookToDelete} onOpenChange={(open) => !open && setBookToDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Kitabı Sil</DialogTitle>
            <DialogDescription>
              Bu kitabı silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek kitap: <span className="font-medium">{bookToDelete?.title}</span>
          </p>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setBookToDelete(null)}>
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => bookToDelete && deleteMutation.mutate(bookToDelete.id)}
            >
              {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

