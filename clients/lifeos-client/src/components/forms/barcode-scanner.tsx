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

// Native BarcodeDetector tip tanımı
declare global {
  interface Window {
    BarcodeDetector?: {
      new (options?: { formats: string[] }): {
        detect(image: ImageBitmap): Promise<Array<{ rawValue: string }>>;
      };
    };
  }
}

export function BarcodeScanner({ onScan, isOpen, onOpenChange }: BarcodeScannerProps) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);
  const codeReader = useRef<BrowserMultiFormatReader | null>(null);

  useEffect(() => {
    // ZXing hazırlığı
    const hints = new Map();
    // ISBN ve olası diğer formatları ekleyelim
    hints.set(DecodeHintType.POSSIBLE_FORMATS, [
      BarcodeFormat.EAN_13, 
      BarcodeFormat.CODE_128
    ]);
    hints.set(DecodeHintType.TRY_HARDER, true);
    
    codeReader.current = new BrowserMultiFormatReader(hints);

    return () => {
      codeReader.current = null;
    };
  }, []);

  // Yardımcı: Resmi Canvas'a çizme (Grayscale + Rotation desteği)
  const drawToCanvas = (
    img: ImageBitmap, 
    canvas: HTMLCanvasElement, 
    rotate: boolean = false
  ): void => {
    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) throw new Error("Canvas context oluşturulamadı");

    // Boyutlandırma (Max 1000px)
    const MAX_DIMENSION = 1000;
    let width = img.width;
    let height = img.height;
    
    const scale = Math.min(1, MAX_DIMENSION / Math.max(width, height));
    width *= scale;
    height *= scale;

    if (rotate) {
      // 90 derece döndür
      canvas.width = height;
      canvas.height = width;
      ctx.save();
      ctx.translate(height / 2, width / 2);
      ctx.rotate(90 * Math.PI / 180);
      ctx.drawImage(img, -width / 2, -height / 2, width, height);
      ctx.restore();
    } else {
      canvas.width = width;
      canvas.height = height;
      ctx.drawImage(img, 0, 0, width, height);
    }

    // Görüntü İyileştirme: Grayscale (Siyah-Beyaz) - Kontrastı artırır
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;
    for (let i = 0; i < data.length; i += 4) {
      const avg = (data[i] + data[i + 1] + data[i + 2]) / 3;
      data[i] = avg;     // Red
      data[i + 1] = avg; // Green
      data[i + 2] = avg; // Blue
      // Alpha (data[i + 3]) değişmez
    }
    ctx.putImageData(imageData, 0, 0);
  };

  // Canvas'ı Image elementine çevirme
  const canvasToImage = (canvas: HTMLCanvasElement): Promise<HTMLImageElement> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = () => reject(new Error("Image oluşturulamadı"));
      img.src = canvas.toDataURL('image/jpeg', 0.85);
    });
  };

  const validateAndComplete = (text: string) => {
    // ISBN Temizleme ve Kontrol
    const cleanText = text.replace(/[^0-9]/g, ''); // Sadece rakamları al
    
    if (cleanText.length === 13 && (cleanText.startsWith('978') || cleanText.startsWith('979'))) {
      onScan(cleanText);
      onOpenChange(false);
    } else {
      setError(`Okunan kod (${text}) geçerli bir kitap ISBN numarası değil.`);
    }
  };

  const processImage = async (file: File) => {
    if (!file) return;

    let imageBitmap: ImageBitmap | null = null;

    try {
      setIsProcessing(true);
      setError(null);
      console.log("1. Dosya alındı:", file.size, "bytes, type:", file.type);

      // Timeout koruması (10 saniye)
      const timeoutPromise = new Promise<never>((_, reject) => 
        setTimeout(() => reject(new Error("İşlem çok uzun sürdü (Zaman aşımı).")), 10000)
      );

      // İşlem mantığı
      const processPromise = async (): Promise<string> => {
        imageBitmap = await createImageBitmap(file);
        console.log("2. Resim yüklendi, boyut:", imageBitmap.width, "x", imageBitmap.height);

        // --- YÖNTEM 1: Native BarcodeDetector (Android/Chrome için Işık Hızında) ---
        if (window.BarcodeDetector) {
          try {
            console.log("3. Native BarcodeDetector deneniyor...");
            const nativeDetector = new window.BarcodeDetector({ 
              formats: ['ean_13', 'code_128'] 
            });
            const barcodes = await nativeDetector.detect(imageBitmap);
            
            if (barcodes.length > 0) {
              const rawValue = barcodes[0].rawValue;
              console.log("✅ Native API Buldu:", rawValue);
              return rawValue;
            }
            console.log("Native dedektör barkod bulamadı, ZXing'e geçiliyor...");
          } catch (e) {
            console.warn("Native dedektör başarısız, ZXing'e geçiliyor...", e);
          }
        }

        // --- YÖNTEM 2: ZXing (Yedek Güç) ---
        if (!codeReader.current) {
          throw new Error("Barkod okuyucu hazır değil");
        }

        const canvas = document.createElement('canvas');
        let foundCode: string | null = null;

        // Deneme 1: Normal Yön
        try {
          console.log("4. ZXing ile normal yönde okuma deneniyor...");
          drawToCanvas(imageBitmap, canvas, false);
          const img = await canvasToImage(canvas);
          const result = await codeReader.current.decodeFromImageElement(img);
          foundCode = result.getText();
          console.log("✅ Normal yönde bulundu:", foundCode);
        } catch {
          console.log("Normal okuma başarısız, döndürüp deneniyor...");
        }

        // Deneme 2: Eğer bulunamadıysa 90 derece çevirip dene (Dikey fotoğraflar için)
        if (!foundCode) {
          try {
            console.log("5. ZXing ile 90 derece döndürülmüş yönde okuma deneniyor...");
            drawToCanvas(imageBitmap, canvas, true); // Rotate = true
            const img = await canvasToImage(canvas);
            const result = await codeReader.current.decodeFromImageElement(img);
            foundCode = result.getText();
            console.log("✅ Döndürülmüş yönde bulundu:", foundCode);
          } catch {
            console.log("Döndürülmüş okuma da başarısız.");
          }
        }

        if (!foundCode) {
          throw new Error("Barkod bulunamadı");
        }

        return foundCode;
      };

      // Yarıştır: Hangisi önce biterse (İşlem mi, Timeout mu?)
      const text = await Promise.race([processPromise(), timeoutPromise]);

      // ISBN Kontrolü ve Tamamlama
      validateAndComplete(text);

    } catch (err: any) {
      console.error("HATA:", err);
      
      // Hata mesajını kullanıcı dostu yap
      if (err.message?.includes("NotFound") || err.message?.includes("not found") || err.message?.includes("bulunamadı")) {
        setError("Fotoğrafta barkod bulunamadı. Lütfen barkodun net, düz ve iyi aydınlatılmış olduğundan emin olun.");
      } else if (err.message?.includes("Zaman aşımı") || err.message?.includes("uzun sürdü")) {
        setError("İşlem çok uzun sürdü. Lütfen daha küçük veya net bir fotoğraf deneyin.");
      } else {
        setError("Barkod okunamadı. Lütfen fotoğrafın net olduğundan ve barkodun parlamadığından emin olun.");
      }
    } finally {
      // Bellek temizliği
      if (imageBitmap && 'close' in imageBitmap) {
        try {
          (imageBitmap as ImageBitmap).close();
        } catch {
          // ImageBitmap.close() desteklenmiyorsa sessizce geç
        }
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
            <p className="text-center text-sm text-muted-foreground mb-2 px-2">
              Kitabın arkasındaki barkodu çekin. <br/>
              <span className="text-xs opacity-75">(Dikey veya yatay fark etmez, biz hallederiz)</span>
            </p>

            {error && (
              <div className="bg-destructive/10 p-3 rounded-lg flex items-start gap-3 border border-destructive/20 animate-in slide-in-from-top-2">
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
                  <p className="font-medium">Analiz Ediliyor...</p>
                  <p className="text-xs text-muted-foreground">Lütfen bekleyin...</p>
                </div>
              </div>
            ) : (
              <>
                <Button 
                  size="lg" 
                  className="w-full h-14 text-lg gap-2 shadow-md"
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
                  Galeriden Yükle
                </Button>
              </>
            )}

            <div className="text-xs text-muted-foreground space-y-1 w-full pt-2 border-t">
              <p>• Mobilde: "Fotoğraf Çek" kamerayı başlatır</p>
              <p>• "Galeriden Yükle" ile daha önce çektiğiniz fotoğrafları kullanabilirsiniz</p>
              <p>• Barkod net, düz ve iyi aydınlatılmış olmalıdır</p>
              <p>• Konsolda (F12) işlem adımlarını görebilirsiniz</p>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
