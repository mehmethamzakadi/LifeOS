import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { PlusCircle, Pencil, Trash2, TrendingUp, TrendingDown, Wallet } from 'lucide-react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { createWalletTransaction, updateWalletTransaction, deleteWalletTransaction, fetchWalletTransactions } from '../../features/wallettransactions/api';
import { WalletTransaction, WalletTransactionFormValues, TransactionType, TransactionCategory, TransactionTypeLabels, TransactionCategoryLabels } from '../../features/wallettransactions/types';
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
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { StatCard } from '../../components/dashboard/stat-card';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { handleApiError, showApiResponseError } from '../../lib/api-error';
import { cn } from '../../lib/utils';

const transactionSchema = z.object({
  title: z.string().min(2, 'Başlık en az 2 karakter olmalıdır').max(200, 'Başlık en fazla 200 karakter olabilir'),
  amount: z.number().min(0.01, 'Tutar 0\'dan büyük olmalıdır'),
  type: z.nativeEnum(TransactionType),
  category: z.nativeEnum(TransactionCategory),
  transactionDate: z.string().min(1, 'Tarih seçiniz')
});

type TransactionFormSchema = z.infer<typeof transactionSchema>;

export function WalletPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize] = useState(20);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingTransaction, setEditingTransaction] = useState<WalletTransaction | null>(null);
  const [transactionToDelete, setTransactionToDelete] = useState<WalletTransaction | null>(null);
  const [selectedType, setSelectedType] = useState<TransactionType | 'all'>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['wallettransactions', pageIndex, pageSize, searchTerm],
    queryFn: () => fetchWalletTransactions({
      search: searchTerm || undefined,
      pageIndex,
      pageSize,
      sort: { field: 'transactionDate', dir: 'desc' }
    })
  });

  const formMethods = useForm<TransactionFormSchema>({
    resolver: zodResolver(transactionSchema),
    defaultValues: {
      title: '',
      amount: 0,
      type: TransactionType.Income,
      category: TransactionCategory.Other,
      transactionDate: new Date().toISOString().split('T')[0]
    }
  });

  useEffect(() => {
    if (editingTransaction) {
      formMethods.reset({
        title: editingTransaction.title,
        amount: Math.abs(editingTransaction.amount),
        type: editingTransaction.type,
        category: editingTransaction.category,
        transactionDate: editingTransaction.transactionDate.split('T')[0]
      });
    } else if (isCreateOpen) {
      // Dialog açıldığında formu reset et
      formMethods.reset({
        title: '',
        amount: 0,
        type: TransactionType.Income,
        category: TransactionCategory.Other,
        transactionDate: new Date().toISOString().split('T')[0]
      });
    }
  }, [editingTransaction, isCreateOpen, formMethods]);

  const createMutation = useMutation({
    mutationFn: createWalletTransaction,
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İşlem eklenemedi');
        return;
      }
      toast.success(result.message || 'İşlem eklendi');
      setIsCreateOpen(false);
      // Formu reset et
      formMethods.reset({
        title: '',
        amount: 0,
        type: TransactionType.Income,
        category: TransactionCategory.Other,
        transactionDate: new Date().toISOString().split('T')[0]
      });
      queryClient.invalidateQueries({ queryKey: ['wallettransactions'] });
    },
    onError: (error) => handleApiError(error, 'İşlem eklenemedi')
  });

  const updateMutation = useMutation({
    mutationFn: (values: WalletTransactionFormValues) =>
      editingTransaction ? updateWalletTransaction(editingTransaction.id, values) : Promise.reject(),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İşlem güncellenemedi');
        return;
      }
      toast.success(result.message || 'İşlem güncellendi');
      setEditingTransaction(null);
      queryClient.invalidateQueries({ queryKey: ['wallettransactions'] });
    },
    onError: (error) => handleApiError(error, 'İşlem güncellenemedi')
  });

  const deleteMutation = useMutation({
    mutationFn: (transactionId: string) => deleteWalletTransaction(transactionId),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'İşlem silinemedi');
        return;
      }
      toast.success(result.message || 'İşlem silindi');
      setTransactionToDelete(null);
      queryClient.invalidateQueries({ queryKey: ['wallettransactions'] });
    },
    onError: (error) => handleApiError(error, 'İşlem silinemedi')
  });

  const onSubmit = formMethods.handleSubmit(async (values) => {
    const amount = values.type === TransactionType.Income 
      ? Math.abs(values.amount) 
      : -Math.abs(values.amount);
    
    const formData: WalletTransactionFormValues = {
      title: values.title,
      amount,
      type: values.type,
      category: values.category,
      transactionDate: values.transactionDate
    };
    if (editingTransaction) {
      await updateMutation.mutateAsync(formData);
    } else {
      await createMutation.mutateAsync(formData);
    }
  });

  const filteredTransactions = useMemo(() => {
    if (!data?.items) return [];
    if (selectedType === 'all') return data.items;
    return data.items.filter(t => t.type === selectedType);
  }, [data, selectedType]);

  const statistics = useMemo(() => {
    if (!data?.items) {
      return {
        totalIncome: 0,
        totalExpense: 0,
        balance: 0
      };
    }
    const income = data.items
      .filter(t => t.type === TransactionType.Income)
      .reduce((sum, t) => sum + Math.abs(t.amount), 0);
    const expense = data.items
      .filter(t => t.type === TransactionType.Expense)
      .reduce((sum, t) => sum + Math.abs(t.amount), 0);
    return {
      totalIncome: income,
      totalExpense: expense,
      balance: income - expense
    };
  }, [data]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY'
    }).format(amount);
  };

  return (
    <div className="space-y-6">
      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <StatCard
          title="Toplam Gelir"
          value={formatCurrency(statistics.totalIncome)}
          description="Tüm gelirlerin toplamı"
          icon={TrendingUp}
          color="green"
        />
        <StatCard
          title="Toplam Gider"
          value={formatCurrency(statistics.totalExpense)}
          description="Tüm giderlerin toplamı"
          icon={TrendingDown}
          color="red"
        />
        <StatCard
          title="Bakiye"
          value={formatCurrency(statistics.balance)}
          description="Net bakiye"
          icon={Wallet}
          color={statistics.balance >= 0 ? 'blue' : 'red'}
        />
      </div>

      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle>Cüzdan İşlemleri</CardTitle>
            <p className="text-sm text-muted-foreground">
              Gelir ve giderlerinizi takip edin.
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
                  amount: 0,
                  type: TransactionType.Income,
                  category: TransactionCategory.Other,
                  transactionDate: new Date().toISOString().split('T')[0]
                });
              }
            }}
          >
            <DialogTrigger asChild>
              <Button className="gap-2">
                <PlusCircle className="h-4 w-4" /> Yeni İşlem
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Yeni İşlem</DialogTitle>
                <DialogDescription>Yeni bir gelir veya gider ekleyin.</DialogDescription>
              </DialogHeader>
              <form id="create-transaction-form" onSubmit={onSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="transaction-title">Başlık *</Label>
                  <Input id="transaction-title" placeholder="İşlem başlığı" {...formMethods.register('title')} />
                  {formMethods.formState.errors.title && (
                    <p className="text-sm text-destructive">{formMethods.formState.errors.title.message}</p>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="transaction-type">Tür</Label>
                    <select
                      id="transaction-type"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('type', { valueAsNumber: true })}
                    >
                      {Object.entries(TransactionTypeLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="transaction-category">Kategori</Label>
                    <select
                      id="transaction-category"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...formMethods.register('category', { valueAsNumber: true })}
                    >
                      {Object.entries(TransactionCategoryLabels).map(([key, label]) => (
                        <option key={key} value={key}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="transaction-amount">Tutar *</Label>
                    <Input
                      id="transaction-amount"
                      type="number"
                      step="0.01"
                      min="0"
                      {...formMethods.register('amount', { valueAsNumber: true })}
                    />
                    {formMethods.formState.errors.amount && (
                      <p className="text-sm text-destructive">{formMethods.formState.errors.amount.message}</p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="transaction-date">Tarih *</Label>
                    <Input
                      id="transaction-date"
                      type="date"
                      {...formMethods.register('transactionDate')}
                    />
                    {formMethods.formState.errors.transactionDate && (
                      <p className="text-sm text-destructive">{formMethods.formState.errors.transactionDate.message}</p>
                    )}
                  </div>
                </div>
              </form>
              <DialogFooter>
                <Button type="button" variant="ghost" onClick={() => setIsCreateOpen(false)}>
                  İptal
                </Button>
                <Button type="submit" form="create-transaction-form" disabled={createMutation.isPending}>
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
                variant={selectedType === TransactionType.Income ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedType(TransactionType.Income)}
              >
                <TrendingUp className="h-4 w-4 mr-2" />
                Gelirler
              </Button>
              <Button
                variant={selectedType === TransactionType.Expense ? 'default' : 'outline'}
                size="sm"
                onClick={() => setSelectedType(TransactionType.Expense)}
              >
                <TrendingDown className="h-4 w-4 mr-2" />
                Giderler
              </Button>
            </div>
            <Input
              placeholder="İşlem ara..."
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
            <div className="overflow-hidden rounded-lg border bg-background">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Tarih</TableHead>
                    <TableHead>Başlık</TableHead>
                    <TableHead>Kategori</TableHead>
                    <TableHead>Tür</TableHead>
                    <TableHead className="text-right">Tutar</TableHead>
                    <TableHead className="text-right">İşlemler</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredTransactions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                        İşlem bulunamadı.
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredTransactions.map((transaction) => (
                      <TableRow key={transaction.id}>
                        <TableCell>
                          {new Date(transaction.transactionDate).toLocaleDateString('tr-TR')}
                        </TableCell>
                        <TableCell className="font-medium">{transaction.title}</TableCell>
                        <TableCell>
                          <Badge variant="secondary">
                            {TransactionCategoryLabels[transaction.category]}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant={transaction.type === TransactionType.Income ? 'default' : 'outline'}
                            className={transaction.type === TransactionType.Expense ? 'border-red-500 text-red-600 dark:text-red-400' : ''}
                          >
                            {transaction.type === TransactionType.Income ? (
                              <TrendingUp className="h-3 w-3 mr-1" />
                            ) : (
                              <TrendingDown className="h-3 w-3 mr-1" />
                            )}
                            {TransactionTypeLabels[transaction.type]}
                          </Badge>
                        </TableCell>
                        <TableCell className={cn(
                          'text-right font-semibold',
                          transaction.type === TransactionType.Income
                            ? 'text-green-600 dark:text-green-400'
                            : 'text-red-600 dark:text-red-400'
                        )}>
                          {transaction.type === TransactionType.Income ? '+' : '-'}
                          {formatCurrency(Math.abs(transaction.amount))}
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex items-center justify-end gap-2">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => setEditingTransaction(transaction)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => setTransactionToDelete(transaction)}
                            >
                              <Trash2 className="h-4 w-4 text-destructive" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingTransaction} onOpenChange={(open) => !open && setEditingTransaction(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>İşlemi Düzenle</DialogTitle>
            <DialogDescription>İşlem bilgilerini güncelleyin.</DialogDescription>
          </DialogHeader>
          <form id="edit-transaction-form" onSubmit={onSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="edit-transaction-title">Başlık *</Label>
              <Input id="edit-transaction-title" {...formMethods.register('title')} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-transaction-type">Tür</Label>
                <select
                  id="edit-transaction-type"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('type', { valueAsNumber: true })}
                >
                  {Object.entries(TransactionTypeLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-transaction-category">Kategori</Label>
                <select
                  id="edit-transaction-category"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  {...formMethods.register('category', { valueAsNumber: true })}
                >
                  {Object.entries(TransactionCategoryLabels).map(([key, label]) => (
                    <option key={key} value={key}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-transaction-amount">Tutar *</Label>
                <Input
                  id="edit-transaction-amount"
                  type="number"
                  step="0.01"
                  min="0"
                  {...formMethods.register('amount', { valueAsNumber: true })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-transaction-date">Tarih *</Label>
                <Input
                  id="edit-transaction-date"
                  type="date"
                  {...formMethods.register('transactionDate')}
                />
              </div>
            </div>
          </form>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setEditingTransaction(null)}>
              İptal
            </Button>
            <Button type="submit" form="edit-transaction-form" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!transactionToDelete} onOpenChange={(open) => !open && setTransactionToDelete(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>İşlemi Sil</DialogTitle>
            <DialogDescription>
              Bu işlemi silmek istediğinizden emin misiniz? İşlem geri alınamaz.
            </DialogDescription>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Silinecek işlem: <span className="font-medium">{transactionToDelete?.title}</span>
          </p>
          <DialogFooter>
            <Button type="button" variant="ghost" onClick={() => setTransactionToDelete(null)}>
              İptal
            </Button>
            <Button
              type="button"
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() => transactionToDelete && deleteMutation.mutate(transactionToDelete.id)}
            >
              {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

