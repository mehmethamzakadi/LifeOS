import { useRef, useState, useEffect } from 'react';
import { BrowserMultiFormatReader, BarcodeFormat, DecodeHintType } from '@zxing/library';
import { Camera, Loader2, Image as ImageIcon, BookOpen, AlertCircle, RotateCcw } from 'lucide-react';
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
  
  // Ref'ler
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);
  const codeReader = useRef<BrowserMultiFormatReader | null>(null);

  // Reader'ı sadece component mount olduğunda bir kez oluştur
  useEffect(() => {
    const hints = new Map();
    hints.set(DecodeHintType.POSSIBLE_FORMATS, [BarcodeFormat.EAN_13]);
    hints.set(DecodeHintType.TRY_HARDER, true);
    
    codeReader.current = new BrowserMultiFormatReader(hints);

    return () => {
      // Cleanup
      codeReader.current = null;
    };
  }, []);

  const processImage = async (file: File) => {
    if (!file || !codeReader.current) return;

    let imageBitmap: ImageBitmap | null = null;
    let imageUrl: string | null = null;

    try {
      setIsProcessing(true);
      setError(null);
      console.log("1. Dosya alındı:", file.size, "bytes, type:", file.type);

      // Timeout koruması (8 saniye)
      const timeoutPromise = new Promise<never>((_, reject) => 
        setTimeout(() => reject(new Error("İşlem çok uzun sürdü (Zaman aşımı).")), 8000)
      );

      // İşlem mantığı
      const processPromise = async (): Promise<string> => {
        // 1. Modern yöntemle resmi yükle (createImageBitmap - Promise tabanlı, daha hızlı)
        console.log("2. Resim yükleniyor...");
        imageBitmap = await createImageBitmap(file);
        console.log("3. Resim yüklendi, boyut:", imageBitmap.width, "x", imageBitmap.height);

        // 2. Canvas oluştur ve resmi küçült (800px genişlik yeterli)
        const canvas = document.createElement('canvas');
        const MAX_WIDTH = 800;
        const scale = Math.min(1, MAX_WIDTH / imageBitmap.width);
        
        canvas.width = imageBitmap.width * scale;
        canvas.height = imageBitmap.height * scale;

        const ctx = canvas.getContext('2d', { willReadFrequently: false });
        if (!ctx) throw new Error("Canvas context oluşturulamadı");

        // Resmi canvas'a çiz (EXIF yönü otomatik düzeltilir)
        ctx.drawImage(imageBitmap, 0, 0, canvas.width, canvas.height);
        console.log("4. Resim canvas'a çizildi, boyut:", canvas.width, "x", canvas.height);

        // 3. Canvas'ı Image elementine çevir (Base64'e çevirmeden, bellek dostu)
        const img = new Image();
        imageUrl = canvas.toDataURL('image/jpeg', 0.85);
        img.src = imageUrl;

        // Image yüklenene kadar bekle
        await new Promise<void>((resolve, reject) => {
          img.onload = () => resolve();
          img.onerror = () => reject(new Error("Image yüklenemedi"));
          // Eğer zaten yüklüyse (cache'den)
          if (img.complete) resolve();
        });

        console.log("5. Image element hazır, barkod okunuyor...");

        // 4. ZXing ile Image elementinden oku (Base64 string yerine Image element - daha hızlı)
        if (!codeReader.current) {
          throw new Error("Barkod okuyucu hazır değil");
        }
        const result = await codeReader.current.decodeFromImageElement(img);
        const text = result.getText();
        
        console.log("6. Barkod okundu:", text);

        return text;
      };

      // Yarıştır: Hangisi önce biterse (İşlem mi, Timeout mu?)
      const text = await Promise.race([processPromise(), timeoutPromise]);

      // ISBN Kontrolü
      if (text.length === 13 && (text.startsWith('978') || text.startsWith('979'))) {
        onScan(text);
        onOpenChange(false);
      } else {
        setError(`Barkod okundu (${text}) ancak bir kitap ISBN numarası (978 veya 979 ile başlamalı) değil.`);
      }

    } catch (err: any) {
      console.error("HATA:", err);
      
      // Hata mesajını kullanıcı dostu yap
      if (err.message?.includes("NotFound") || err.message?.includes("not found")) {
        setError("Fotoğrafta barkod bulunamadı. Lütfen barkodun net, düz ve iyi aydınlatılmış olduğundan emin olun.");
      } else if (err.message?.includes("Zaman aşımı") || err.message?.includes("uzun sürdü")) {
        setError("İşlem çok uzun sürdü. Lütfen daha küçük veya net bir fotoğraf deneyin.");
      } else {
        setError("Bir hata oluştu: " + (err.message || "Bilinmeyen hata"));
      }
    } finally {
      // Bellek temizliği
      if (imageBitmap) {
        try {
          (imageBitmap as ImageBitmap).close();
        } catch {
          // ImageBitmap.close() desteklenmiyorsa sessizce geç
        }
      }
      if (imageUrl) {
        // Image URL'i temizle (memory leak önleme)
        const img = new Image();
        img.src = imageUrl;
        img.src = ''; // URL'i temizle
      }
      
      setIsProcessing(false);
      // Inputları sıfırla
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
          {/* Gizli Inputlar */}
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
            <div className="text-center mb-2 px-2">
               <p className="text-sm text-muted-foreground">
                Kitabın arkasındaki barkodun fotoğrafını çekin. 
                <br/><span className="text-xs opacity-70">(Fotoğraf net ve yatay olmalıdır)</span>
              </p>
            </div>

            {error && (
              <div className="bg-destructive/10 p-3 rounded-md flex items-start gap-3 border border-destructive/20">
                <AlertCircle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
                <div className="flex-1 space-y-2">
                  <p className="text-sm text-destructive font-medium">{error}</p>
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="h-7 text-xs border-destructive/30 hover:bg-destructive/20 text-destructive"
                    onClick={() => setError(null)}
                  >
                    <RotateCcw className="w-3 h-3 mr-1" /> Tekrar Dene
                  </Button>
                </div>
              </div>
            )}

            {isProcessing ? (
              <div className="flex flex-col items-center justify-center py-8 gap-3 bg-muted/30 rounded-lg border-2 border-dashed">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <div className="text-center">
                    <p className="font-medium">Fotoğraf Analiz Ediliyor...</p>
                    <p className="text-xs text-muted-foreground">Lütfen bekleyin...</p>
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
                  Fotoğraf Çek
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
                  Galeriden Seç
                </Button>
              </>
            )}

            <div className="text-xs text-muted-foreground space-y-1 w-full pt-2 border-t">
              <p>• Mobilde: "Fotoğraf Çek" kamerayı başlatır</p>
              <p>• "Galeriden Seç" ile daha önce çektiğiniz fotoğrafları kullanabilirsiniz</p>
              <p>• Barkod net, düz ve iyi aydınlatılmış olmalıdır</p>
              <p>• Konsolda (F12) işlem adımlarını görebilirsiniz</p>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
