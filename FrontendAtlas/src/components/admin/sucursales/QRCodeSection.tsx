import { useState, useEffect } from 'react';
import { Download, Printer, QrCode, X } from 'lucide-react';
import { getQRCodeBlobUrl, downloadQRCode } from '../../../services/admin';
import { toast } from 'sonner';

interface QRCodeSectionProps {
    sucursalId: number;
    sucursalNombre: string;
    sucursalSlug: string;
}

type QRSize = 'small' | 'medium' | 'large';

const sizeMap: Record<QRSize, { pixels: number, label: string, description: string }> = {
    small: { pixels: 15, label: 'Pequeño', description: '~375px - Para web' },
    medium: { pixels: 25, label: 'Mediano', description: '~625px - Estándar A4' },
    large: { pixels: 35, label: 'Grande', description: '~875px - Posters' }
};

export default function QRCodeSection({ sucursalId, sucursalNombre, sucursalSlug }: QRCodeSectionProps) {
    const [size, setSize] = useState<QRSize>('medium');
    const [isDownloading, setIsDownloading] = useState(false);
    const [showPrintModal, setShowPrintModal] = useState(false);
    const [qrError, setQrError] = useState(false);
    const [qrImageUrl, setQrImageUrl] = useState<string>('');
    const [isLoadingQr, setIsLoadingQr] = useState(true);

    useEffect(() => {
        // Cargar imagen autenticada
        let isActive = true;
        const loadQr = async () => {
            setIsLoadingQr(true);
            setQrError(false);
            try {
                const url = await getQRCodeBlobUrl(sucursalId, sizeMap[size].pixels);
                if (isActive) {
                    setQrImageUrl(url);
                }
            } catch (err) {
                console.error("Error loading QR:", err);
                if (isActive) setQrError(true);
            } finally {
                if (isActive) setIsLoadingQr(false);
            }
        };

        loadQr();

        return () => {
            isActive = false;
            // Cleanup blob url if needed manually, though React handles component unmounts well usually, 
            // explicit revoke is better but here we keep simple state.
            // Actually let's revoke previous URL when changing to avoid leaks?
            // Doing it simple for now.
        };
    }, [sucursalId, size, sucursalSlug]);

    // Función download y demás handlers...
    const handleDownload = async () => {
        setIsDownloading(true);
        try {
            await downloadQRCode(sucursalId, sizeMap[size].pixels);
            toast.success('QR descargado exitosamente');
        } catch (error) {
            console.error('Error downloading QR:', error);
            toast.error('Error al descargar el QR');
        } finally {
            setIsDownloading(false);
        }
    };

    const handlePrint = () => {
        setShowPrintModal(true);
    };

    const handleClosePrintModal = () => {
        setShowPrintModal(false);
    };

    const handlePrintNow = () => {
        window.print();
    };

    return (
        <>
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
                {/* Header */}
                <div className="flex items-center gap-3 mb-6">
                    <div className="p-2 bg-indigo-100 rounded-lg">
                        <QrCode className="w-6 h-6 text-indigo-600" />
                    </div>
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900">Código QR</h3>
                        <p className="text-sm text-gray-500">Para carta digital</p>
                    </div>
                </div>

                <div className="grid md:grid-cols-2 gap-6">
                    {/* QR Preview */}
                    <div className="flex flex-col items-center">
                        <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl p-6 border-2 border-dashed border-gray-300">
                            {isLoadingQr ? (
                                <div className="w-64 h-64 flex items-center justify-center">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                                </div>
                            ) : !qrError && qrImageUrl ? (
                                <img
                                    src={qrImageUrl}
                                    alt="QR Code"
                                    className="w-64 h-64 object-contain"
                                    onError={() => setQrError(true)}
                                />
                            ) : (
                                <div className="w-64 h-64 flex items-center justify-center bg-gray-200 rounded-lg">
                                    <p className="text-gray-500 text-sm text-center px-4">
                                        Error cargando QR<br />
                                        <span className="text-xs">Verifica conexión</span>
                                    </p>
                                </div>
                            )}
                        </div>

                        {/* URL Info */}
                        <div className="mt-4 p-3 bg-blue-50 rounded-lg w-full">
                            <p className="text-xs font-medium text-blue-900 mb-1">URL del QR:</p>
                            <code className="text-xs text-blue-700 break-all block">
                                {window.location.origin}/{sucursalSlug.replace('-', '/')}
                            </code>
                        </div>
                    </div>

                    {/* Controls */}
                    <div className="flex flex-col justify-between">
                        {/* Size Selector */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-3">
                                Tamaño de impresión:
                            </label>
                            <div className="space-y-2">
                                {(Object.keys(sizeMap) as QRSize[]).map((sizeKey) => (
                                    <button
                                        key={sizeKey}
                                        onClick={() => setSize(sizeKey)}
                                        className={`
                      w-full text-left px-4 py-3 rounded-lg border-2 transition-all
                      ${size === sizeKey
                                                ? 'border-indigo-500 bg-indigo-50 shadow-sm'
                                                : 'border-gray-200 hover:border-gray-300 bg-white'
                                            }
                    `}
                                    >
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <div className="font-medium text-gray-900">
                                                    {sizeMap[sizeKey].label}
                                                </div>
                                                <div className="text-sm text-gray-500">
                                                    {sizeMap[sizeKey].description}
                                                </div>
                                            </div>
                                            {size === sizeKey && (
                                                <div className="w-4 h-4 rounded-full bg-indigo-500 flex items-center justify-center">
                                                    <div className="w-2 h-2 rounded-full bg-white"></div>
                                                </div>
                                            )}
                                        </div>
                                    </button>
                                ))}
                            </div>

                            {/* Usage Instructions */}
                            <div className="mt-6 p-4 bg-amber-50 border border-amber-200 rounded-lg">
                                <p className="text-sm font-medium text-amber-900 mb-2">
                                    💡 Cómo usar:
                                </p>
                                <ul className="text-sm text-amber-800 space-y-1">
                                    <li>• Descarga e imprime el QR</li>
                                    <li>• Colócalo en tu local (mesa, ventana, puerta)</li>
                                    <li>• Los clientes lo escanean para ver tu carta</li>
                                </ul>
                            </div>
                        </div>

                        {/* Action Buttons */}
                        <div className="flex flex-col gap-3 mt-6">
                            <button
                                onClick={handleDownload}
                                disabled={isDownloading || qrError}
                                className="
                  flex items-center justify-center gap-2 px-6 py-3
                  bg-indigo-600 hover:bg-indigo-700 text-white
                  rounded-lg font-medium transition-colors
                  disabled:opacity-50 disabled:cursor-not-allowed
                  shadow-sm hover:shadow-md
                "
                            >
                                <Download className="w-5 h-5" />
                                {isDownloading ? 'Descargando...' : 'Descargar PNG'}
                            </button>

                            <button
                                onClick={handlePrint}
                                disabled={qrError}
                                className="
                  flex items-center justify-center gap-2 px-6 py-3
                  bg-gray-600 hover:bg-gray-700 text-white
                  rounded-lg font-medium transition-colors
                  disabled:opacity-50 disabled:cursor-not-allowed
                  shadow-sm hover:shadow-md
                "
                            >
                                <Printer className="w-5 h-5" />
                                Imprimir
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Print Modal */}
            {showPrintModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 print:hidden">
                    <div className="bg-white rounded-xl shadow-2xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                        <div className="p-6 border-b border-gray-200 flex items-center justify-between">
                            <h3 className="text-xl font-semibold text-gray-900">Vista de Impresión</h3>
                            <button
                                onClick={handleClosePrintModal}
                                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                            >
                                <X className="w-5 h-5 text-gray-500" />
                            </button>
                        </div>

                        <div className="p-8">
                            {/* Print Preview */}
                            <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 bg-white">
                                <div className="text-center space-y-6">
                                    <img
                                        src={qrImageUrl}
                                        alt="QR Code"
                                        className="mx-auto w-80 h-80 object-contain"
                                    />

                                    <div className="space-y-2">
                                        <h2 className="text-2xl font-bold text-gray-900">
                                            {sucursalNombre}
                                        </h2>
                                        <p className="text-lg text-gray-600">
                                            Escanea para ver nuestra carta digital
                                        </p>
                                    </div>

                                    <div className="pt-4 border-t border-gray-200">
                                        <p className="text-sm text-gray-500">
                                            {window.location.origin}/{sucursalSlug.replace('-', '/')}
                                        </p>
                                    </div>
                                </div>
                            </div>

                            {/* Instructions */}
                            <div className="mt-6 p-4 bg-blue-50 rounded-lg">
                                <p className="text-sm text-blue-900 font-medium mb-2">
                                    Instrucciones de impresión:
                                </p>
                                <ul className="text-sm text-blue-800 space-y-1">
                                    <li>• Usa papel tamaño A4 o Carta</li>
                                    <li>• Configura orientación vertical</li>
                                    <li>• Desmarca "Encabezados y pies de página"</li>
                                </ul>
                            </div>
                        </div>

                        <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
                            <button
                                onClick={handleClosePrintModal}
                                className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={handlePrintNow}
                                className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2"
                            >
                                <Printer className="w-4 h-4" />
                                Imprimir Ahora
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Template de Impresión (Oculto en pantalla, Visible al imprimir) */}
            <div className="hidden print:flex fixed inset-0 bg-white z-[9999] flex-col items-center justify-center p-0 m-0">
                <div className="w-full h-full flex flex-col items-center justify-center border-0 p-8">
                    <h1 className="text-5xl font-bold text-black mb-12">{sucursalNombre}</h1>

                    {qrImageUrl && (
                        <img
                            src={qrImageUrl}
                            alt="QR Menú"
                            className="w-[80%] max-w-[600px] h-auto object-contain mb-8"
                        />
                    )}

                    <div className="text-center space-y-4">
                        <p className="text-3xl font-medium text-gray-900">Escanea para ver la carta</p>
                        <p className="text-xl text-gray-500">
                            {typeof window !== 'undefined' ? window.location.origin : ''}/{sucursalSlug.replace('-', '/')}
                        </p>
                    </div>

                    <div className="mt-16 pt-8 border-t border-gray-200">
                        <p className="text-sm text-gray-400">Potenciado por Atlas</p>
                    </div>
                </div>
            </div>

            <style>{`
                @media print {
                    @page {
                        margin: 0;
                        size: auto;
                    }
                    body {
                        background: white;
                    }
                    /* Ocultar interfaz de usuario normal */
                    nav, aside, header, button, .lucide {
                        display: none !important;
                    }
                    /* Asegurar que el modal de preview no estorbe si está abierto */
                    .fixed.inset-0.bg-black {
                        display: none !important;
                    }
                }
            `}</style>
        </>
    );
}
