import { AlertCircle, RefreshCw } from 'lucide-react';

export function ErrorMessage({ 
  message = 'Ocorreu um erro inesperado', 
  onRetry,
  className 
}) {
  return (
    <div className={`bg-red-50 border border-red-200 rounded-lg p-4 ${className || ''}`}>
      <div className="flex items-center">
        <AlertCircle className="h-5 w-5 text-red-600 mr-2" />
        <span className="text-red-800">{message}</span>
        {onRetry && (
          <button
            onClick={onRetry}
            className="ml-auto inline-flex items-center px-3 py-1.5 text-sm text-red-600 hover:text-red-700 hover:bg-red-100 rounded transition-colors"
          >
            <RefreshCw className="h-4 w-4 mr-1" />
            Tentar novamente
          </button>
        )}
      </div>
    </div>
  );
}

export function ErrorPage({ 
  message = 'Ocorreu um erro ao carregar a página', 
  onRetry 
}) {
  return (
    <div className="flex h-64 items-center justify-center">
      <div className="text-center space-y-4">
        <AlertCircle className="h-12 w-12 text-red-600 mx-auto" />
        <div>
          <h3 className="text-lg font-semibold text-gray-900">Erro</h3>
          <p className="text-gray-600">{message}</p>
        </div>
        {onRetry && (
          <button
            onClick={onRetry}
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Tentar novamente
          </button>
        )}
      </div>
    </div>
  );
}

export default ErrorMessage;

