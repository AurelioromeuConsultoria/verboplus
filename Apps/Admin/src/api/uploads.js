import { api } from '@/lib/apiClient';

export const uploadApi = {
  uploadImage: (formData) =>
    api.post('/admin/upload/images', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }),
  uploadVideo: (formData) =>
    api.post('/admin/upload/videos', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }),
  uploadAudio: (formData) =>
    api.post('/admin/upload/audios', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }),
  uploadFile: (formData) =>
    api.post('/admin/upload/files', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }),
};

