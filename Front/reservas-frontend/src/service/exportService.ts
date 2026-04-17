import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import * as XLSX from 'xlsx';

interface ExportOptions {
    reportName: string;
    fileName: string;
    filters?: {
        periodo?: string;
        sucursal?: string;
        fechaDesde?: string;
        fechaHasta?: string;
        [key: string]: any;
    };
}

const LOGO_PATH = '/Logofarmacia.png';
const BRAND_NAME = 'Farmacia General Paz';
const BRAND_COLOR = '#1f2937'; // gray-900
const ACCENT_COLOR = '#8b5cf6'; // purple-500

/**
 * Obtiene el nombre del usuario desde localStorage
 */
const getUserName = (): string => {
    try {
        // Intenta leer el usuario completo
        const userData = localStorage.getItem('user');
        if (userData) {
            const user = JSON.parse(userData);
            // Intenta con diferentes campos posibles
            const nombre = user.nombre || user.name || user.nombreUsuario || user.nombreCompleto || 'Usuario';
            return nombre.trim() || 'Usuario';
        }
        
        // Si no está en 'user', intenta otros keys
        const token = localStorage.getItem('farmacia_token');
        if (token) {
            try {
                // Decodifica el JWT para extraer el nombre
                const payload = JSON.parse(atob(token.split('.')[1]));
                return payload.nombre || payload.name || 'Usuario';
            } catch {
                return 'Usuario';
            }
        }
    } catch {
        return 'Usuario';
    }
    return 'Usuario';
};

/**
 * Formatea la fecha actual
 */
const getFormattedDate = (): string => {
    const now = new Date();
    const day = String(now.getDate()).padStart(2, '0');
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const year = now.getFullYear();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
};

/**
 * Construye el texto de los filtros aplicados
 */
const getFiltersText = (filters?: ExportOptions['filters']): string => {
    if (!filters) return '';

    const filterParts: string[] = [];

    if (filters.periodo) {
        filterParts.push(`Período: ${filters.periodo}`);
    }
    if (filters.sucursal) {
        filterParts.push(`Sucursal: ${filters.sucursal}`);
    }
    if (filters.fechaDesde || filters.fechaHasta) {
        if (filters.fechaDesde && filters.fechaHasta) {
            filterParts.push(`Fechas: ${filters.fechaDesde} a ${filters.fechaHasta}`);
        } else if (filters.fechaDesde) {
            filterParts.push(`Desde: ${filters.fechaDesde}`);
        } else if (filters.fechaHasta) {
            filterParts.push(`Hasta: ${filters.fechaHasta}`);
        }
    }

    return filterParts.length > 0 ? filterParts.join(' | ') : '';
};

/**
 * Exporta datos tabulares a Excel
 * @param data Array de objetos con los datos
 * @param options Opciones de exportación
 */
export const exportToExcel = (
    data: any[],
    options: ExportOptions
) => {
    try {
        // Crear worksheet con los datos
        const ws = XLSX.utils.json_to_sheet(data);

        // Ajustar ancho de columnas automáticamente
        const maxWidth = 50;
        const colWidths: number[] = [];
        
        if (data.length > 0) {
            Object.keys(data[0]).forEach((key) => {
                colWidths.push(Math.min(maxWidth, key.length + 5));
            });
            ws['!cols'] = colWidths.map(w => ({ wch: w }));
        }

        // Crear workbook y agregar metadatos
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Reporte');

        // Agregar hoja de información
        const infoWs = XLSX.utils.aoa_to_sheet([
            [BRAND_NAME],
            [''],
            ['Reporte:', options.reportName],
            ['Fecha de Exportación:', getFormattedDate()],
            ['Usuario:', getUserName()],
            ...(options.filters ? [['Filtros:', getFiltersText(options.filters)]] : []),
        ]);
        
        infoWs['!cols'] = [{ wch: 25 }, { wch: 50 }];
        XLSX.utils.book_append_sheet(wb, infoWs, 'Información');

        // Descargar archivo
        const timestamp = new Date().getTime();
        XLSX.writeFile(wb, `${options.fileName}_${timestamp}.xlsx`);
    } catch (error) {
        console.error('Error exporting to Excel:', error);
        alert('Error al exportar a Excel');
    }
};

/**
 * Exporta a PDF con cabecera personalizada
 * @param htmlElement Elemento HTML a capturar
 * @param options Opciones de exportación
 */
export const exportToPDF = async (
    htmlElement: HTMLElement | null,
    options: ExportOptions
): Promise<void> => {
    if (!htmlElement) {
        alert('No hay contenido para exportar');
        return;
    }

    try {
        // Capturar con mejor resolución y sin elementos de filtro borrosos
        const canvas = await html2canvas(htmlElement, {
            scale: 3, // Mayor resolución
            useCORS: true,
            backgroundColor: '#ffffff',
            logging: false,
            allowTaint: true,
            imageTimeout: 5000,
        });

        const imgData = canvas.toDataURL('image/png');
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4',
        });

        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();
        
        // Márgenes
        const margin = 10;
        const contentWidth = pageWidth - margin * 2;
        
        // Ajustar altura proporcionalmente
        const scaleFactor = contentWidth / canvas.width;
        const contentHeight = canvas.height * scaleFactor;

        // Agregar cabecera
        addPDFHeader(pdf, options);

        // Posición inicial después de la cabecera
        let yPosition = 35;
        
        // Si el contenido es muy largo, dividir en páginas
        const maxHeightPerPage = pageHeight - yPosition - margin;
        let remainingHeight = contentHeight;
        let currentPage = 1;
        let sourceY = 0;

        while (remainingHeight > 0) {
            const heightToPrint = Math.min(remainingHeight, maxHeightPerPage);
            
            // Crear canvas parcial si es necesario
            if (currentPage > 1) {
                pdf.addPage();
                yPosition = margin;
            }

            // Dibujar porción de imagen
            const sliceCanvas = document.createElement('canvas');
            sliceCanvas.width = canvas.width;
            sliceCanvas.height = Math.min(canvas.height, (heightToPrint / scaleFactor));
            
            const ctx = sliceCanvas.getContext('2d');
            if (ctx) {
                ctx.drawImage(
                    canvas,
                    0,
                    sourceY,
                    canvas.width,
                    sliceCanvas.height,
                    0,
                    0,
                    canvas.width,
                    sliceCanvas.height
                );
            }

            const sliceImgData = sliceCanvas.toDataURL('image/png');
            pdf.addImage(sliceImgData, 'PNG', margin, yPosition, contentWidth, heightToPrint);

            remainingHeight -= heightToPrint;
            sourceY += sliceCanvas.height;
            currentPage++;
            
            addPDFFooter(pdf, currentPage.toString());
        }

        // Descargar
        const timestamp = new Date().getTime();
        pdf.save(`${options.fileName}_${timestamp}.pdf`);
    } catch (error) {
        console.error('Error exporting to PDF:', error);
        alert('Error al exportar a PDF');
    }
};

/**
 * Agrega cabecera personalizada al PDF
 */
const addPDFHeader = (pdf: jsPDF, options: ExportOptions) => {
    const pageWidth = pdf.internal.pageSize.getWidth();
    const margin = 10;

    // Color de fondo para encabezado
    pdf.setFillColor(31, 41, 55); // gray-900
    pdf.rect(0, 0, pageWidth, 33, 'F');

    // Logo (requiere estar en public)
    try {
        pdf.addImage(LOGO_PATH, 'PNG', margin, 3, 8, 8);
    } catch {
        // Si el logo no se carga, continuamos sin él
    }

    // Textos del encabezado
    pdf.setTextColor(255, 255, 255);
    pdf.setFontSize(14);
    pdf.setFont('helvetica', 'bold');
    pdf.text(BRAND_NAME, pageWidth / 2, 9, { align: 'center' });

    pdf.setFontSize(10);
    pdf.setFont('helvetica', 'normal');
    pdf.text(`Reporte: ${options.reportName}`, margin, 18);
    pdf.text(`Fecha: ${getFormattedDate()}`, margin, 22);

    if (options.filters) {
        const filtersText = getFiltersText(options.filters);
        if (filtersText) {
            pdf.text(`Filtros: ${filtersText}`, margin, 26);
        }
    }

    pdf.text(`Usuario: ${getUserName()}`, pageWidth - margin, 18, { align: 'right' });
};

/**
 * Agrega pie de página al PDF
 */
const addPDFFooter = (pdf: jsPDF, pageNumber: string) => {
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    const margin = 10;

    pdf.setTextColor(128, 92, 246); // purple
    pdf.setFontSize(8);
    pdf.text(
        `Página ${pageNumber}`,
        pageWidth / 2,
        pageHeight - margin / 2,
        { align: 'center' }
    );
};

/**
 * Exporta tabla HTML a Excel formateado
 * Solo toma datos de tabla HTML
 */
export const exportTableToExcel = (
    tableElement: HTMLElement | null,
    options: ExportOptions
) => {
    if (!tableElement) {
        alert('No hay tabla para exportar');
        return;
    }

    try {
        // Extraer datos de la tabla
        const ws = XLSX.utils.table_to_sheet(tableElement);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Datos');

        // Agregar hoja de información
        const infoWs = XLSX.utils.aoa_to_sheet([
            [BRAND_NAME],
            [''],
            ['Reporte:', options.reportName],
            ['Fecha de Exportación:', getFormattedDate()],
            ['Usuario:', getUserName()],
            ...(options.filters ? [['Filtros:', getFiltersText(options.filters)]] : []),
        ]);
        
        infoWs['!cols'] = [{ wch: 25 }, { wch: 50 }];
        XLSX.utils.book_append_sheet(wb, infoWs, 'Información');

        const timestamp = new Date().getTime();
        XLSX.writeFile(wb, `${options.fileName}_${timestamp}.xlsx`);
    } catch (error) {
        console.error('Error exporting table to Excel:', error);
        alert('Error al exportar tabla a Excel');
    }
};
