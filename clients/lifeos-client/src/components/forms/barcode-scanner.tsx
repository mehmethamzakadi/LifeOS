import { useRef, useState } from 'react';
import { BrowserMultiFormatReader, BarcodeFormat, DecodeHintType } from '@zxing/library';
import { Camera, Loader2, Image as ImageIcon, BookOpen, RotateCcw, AlertCircle } from 'lucide-react';
import { Button } from '../ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/dialog';

interface BarcodeScannerProps {
  onScan: (isbn: string) => void;
  onClose?: () => void;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function BarcodeScanner({ onScan, isOpen, onOpenChange }: BarcodeScannerProps) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  // Görseli küçültme fonksiyonu (Performance Killer önleyici)
  const resizeImage = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const objectUrl = URL.createObjectURL(file);
      img.src = objectUrl;
      
      img.onload = () => {
        const canvas = document.createElement('canvas');
        const MAX_WIDTH = 1000; // 1000px yeterli (4000px işlemciyi yorar)
        const scaleSize = MAX_WIDTH / img.width;
        
        // Eğer resim zaten küçükse boyutlandırma
        if (scaleSize >= 1) {
             canvas.width = img.width;
             canvas.height = img.height;
        } else {
             canvas.width = MAX_WIDTH;
             canvas.height = img.height * scaleSize;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) {
            URL.revokeObjectURL(objectUrl);
            reject(new Error("Canvas context oluşturulamadı"));
            return;
        }
        
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        
        // JPEG formatında ve biraz sıkıştırılmış döndür
        const dataUrl = canvas.toDataURL('image/jpeg', 0.8);
        URL.revokeObjectURL(objectUrl);
        resolve(dataUrl);
      };
      
      img.onerror = (err) => {
        URL.revokeObjectURL(objectUrl);
        reject(err);
      };
    });
  };

  const processImage = async (file: File) => {
    if (!file) return;

    try {
      setIsProcessing(true);
      setError(null);
      console.log("1. Dosya alındı:", file.size, "bytes, type:", file.type);

      // Timeout koruması (10 saniye sürerse iptal et)
      const timeoutPromise = new Promise<never>((_, reject) => 
        setTimeout(() => reject(new Error("İşlem çok uzun sürdü (Zaman aşımı).")), 10000)
      );

      // İşlem mantığı
      const processPromise = async (): Promise<string> => {
        // A. Resmi küçült
        console.log("2. Resim küçültülüyor...");
        const resizedImageUrl = await resizeImage(file);
        console.log("3. Resim küçültüldü, okuma başlıyor.");

        // B. ZXing Ayarları
        const hints = new Map();
        hints.set(DecodeHintType.POSSIBLE_FORMATS, [BarcodeFormat.EAN_13]);
        hints.set(DecodeHintType.TRY_HARDER, true); // Daha detaylı tara
        
        const codeReader = new BrowserMultiFormatReader(hints);
        
        // C. Resimden oku
        const result = await codeReader.decodeFromImageUrl(resizedImageUrl);
        return result.getText();
      };

      // Yarıştır: Hangisi önce biterse (İşlem mi, Timeout mu?)
      const text = await Promise.race([processPromise(), timeoutPromise]);

      console.log("4. Sonuç bulundu:", text);

      // ISBN Kontrolü
      if (text.length === 13 && (text.startsWith('978') || text.startsWith('979'))) {
        onScan(text);
        onOpenChange(false);
      } else {
        setError(`Barkod okundu (${text}) fakat bir kitap ISBN numarası (978...) değil.`);
      }

    } catch (err: any) {
      console.error("HATA:", err);
      
      // Hata mesajını kullanıcı dostu yap
      if (err.message?.includes("NotFound") || err.message?.includes("not found")) {
        setError("Fotoğrafta barkod bulunamadı. Lütfen barkodun net ve düz olduğundan emin olun.");
      } else if (err.message?.includes("Zaman aşımı") || err.message?.includes("uzun sürdü")) {
        setError("İşlem çok uzun sürdü. Lütfen daha küçük veya net bir fotoğraf deneyin.");
      } else {
        setError("Bir hata oluştu: " + (err.message || "Bilinmeyen hata"));
      }
    } finally {
      setIsProcessing(false);
      // Inputları temizle
      if (cameraInputRef.current) cameraInputRef.current.value = '';
      if (galleryInputRef.current) galleryInputRef.current.value = '';
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) processImage(file);
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md mx-4 select-none">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <BookOpen className="h-5 w-5" />
            Kitap Barkodu Tara
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-4">
          
          <input
            ref={cameraInputRef}
            type="file"
            accept="image/*"
            capture="environment" 
            className="hidden"
            onChange={handleFileChange}
          />

          <input
            ref={galleryInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleFileChange}
          />

          <div className="flex flex-col gap-3">
            <div className="text-center mb-2">
               <p className="text-sm text-muted-foreground px-2">
                Barkodun fotoğrafını çekin. Sistem otomatik olarak ISBN numarasını bulacaktır.
              </p>
            </div>

            {error && (
              <div className="flex flex-col gap-2 p-3 bg-destructive/10 border border-destructive/20 rounded-lg text-center">
                <div className="flex items-start gap-2">
                  <AlertCircle className="h-4 w-4 text-destructive mt-0.5 shrink-0" />
                  <p className="text-sm text-destructive flex-1">{error}</p>
                </div>
                <Button 
                    variant="outline" 
                    size="sm" 
                    className="h-8 border-destructive/20 hover:bg-destructive/10 text-destructive"
                    onClick={() => setError(null)}
                >
                    <RotateCcw className="w-3 h-3 mr-1" /> Tamam
                </Button>
              </div>
            )}

            {isProcessing ? (
              <div className="flex flex-col items-center justify-center py-8 gap-3 bg-muted/30 rounded-lg border-2 border-dashed">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <div className="text-center">
                    <p className="font-medium">Fotoğraf İşleniyor...</p>
                    <p className="text-xs text-muted-foreground">Büyük dosyalar için 3-5 saniye sürebilir.</p>
                </div>
              </div>
            ) : (
              <>
                <Button 
                  size="lg" 
                  className="w-full h-14 text-lg gap-2 shadow-sm"
                  onClick={() => cameraInputRef.current?.click()}
                >
                  <Camera className="h-6 w-6" />
                  Kamerayı Aç
                </Button>

                <div className="relative py-1">
                    <div className="absolute inset-0 flex items-center">
                        <span className="w-full border-t" />
                    </div>
                    <div className="relative flex justify-center text-xs uppercase">
                        <span className="bg-background px-2 text-muted-foreground">veya</span>
                    </div>
                </div>

                <Button 
                  variant="outline" 
                  className="w-full h-12 text-base gap-2"
                  onClick={() => galleryInputRef.current?.click()}
                >
                  <ImageIcon className="h-5 w-5" />
                  Galeriden Fotoğraf Seç
                </Button>
              </>
            )}

            <div className="text-xs text-muted-foreground space-y-1 w-full pt-2 border-t">
              <p>• Mobilde: "Kamerayı Aç" kamerayı başlatır</p>
              <p>• "Galeriden Seç" ile daha önce çektiğiniz fotoğrafları kullanabilirsiniz</p>
              <p>• Barkod net ve iyi aydınlatılmış olmalıdır</p>
              <p>• Konsolda (F12) işlem adımlarını görebilirsiniz</p>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
