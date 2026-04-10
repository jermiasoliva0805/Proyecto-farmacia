import React from 'react';
import { Download, FileText } from 'lucide-react';

interface ExportDialogProps {
    isOpen: boolean;
    reportName: string;
    onExcelClick: () => void;
    onPdfClick: () => void;
    onCancel: () => void;
    isLoading?: boolean;
}

export const ExportDialog: React.FC<ExportDialogProps> = ({
    isOpen,
    reportName,
    onExcelClick,
    onPdfClick,
    onCancel,
    isLoading = false,
}) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-sm w-full mx-4">
                {/* Header */}
                <div className="mb-6">
                    <h2 className="text-xl font-bold text-gray-800 mb-2">Exportar Reporte</h2>
                    <p className="text-sm text-gray-600">
                        {reportName}
                    </p>
                </div>

                {/* Divider */}
                <div className="h-px bg-gray-200 mb-6"></div>

                {/* Message */}
                <p className="text-gray-700 text-sm mb-6 text-center">
                    Elige el formato en el que deseas descargar el reporte:
                </p>

                {/* Buttons */}
                <div className="flex gap-3">
                    {/* Excel Button */}
                    <button
                        onClick={onExcelClick}
                        disabled={isLoading}
                        className="flex-1 bg-green-500 hover:bg-green-600 disabled:bg-gray-400 text-white px-4 py-3 rounded-lg font-semibold text-sm flex items-center justify-center gap-2 transition-all shadow-md"
                    >
                        <FileText size={18} />
                        Excel
                    </button>

                    {/* PDF Button */}
                    <button
                        onClick={onPdfClick}
                        disabled={isLoading}
                        className="flex-1 bg-red-500 hover:bg-red-600 disabled:bg-gray-400 text-white px-4 py-3 rounded-lg font-semibold text-sm flex items-center justify-center gap-2 transition-all shadow-md"
                    >
                        <Download size={18} />
                        PDF
                    </button>
                </div>

                {/* Cancel Button */}
                <button
                    onClick={onCancel}
                    disabled={isLoading}
                    className="w-full mt-3 bg-gray-200 hover:bg-gray-300 disabled:bg-gray-300 text-gray-700 px-4 py-2 rounded-lg font-medium text-sm transition-all"
                >
                    Cancelar
                </button>

                {/* Loading state */}
                {isLoading && (
                    <div className="mt-4 text-center">
                        <p className="text-sm text-gray-600 animate-pulse">Generando archivo...</p>
                    </div>
                )}
            </div>
        </div>
    );
};
