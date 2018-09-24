import * as types from '../constants/actionTypes'
import request from '../httpConfig'


const importFileSuccess = file => (
    {
        type: types.IMPORT_FILE_SUCCESS,
        file
    }
);

const getAllFilesForTaskSuccess = files => (
    {
        type: types.GET_ALL_TASKS_SUCCESS,
        files
    }
);

const importFile = (data, {taskId}) => (
    async dispatch => {
        try {
            const res = await request.post(`/api/parser/upload/${taskId}`, data);
            dispatch(importFileSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const getAllFilesForTask = taskId => (
    async dispatch => {
        try {
            const res = await request.get(`/api/parser/getFiles?taskId=${taskId}`);
            dispatch(getAllFilesForTaskSuccess(res.data));
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const getFileDetails = ({recordId, contactId, taskId}) => (
    async dispatch => {
        try {
            const res = await request.get(`/api/parser/getFile?fileId=${recordId}&contactId=${contactId}&taskId=${taskId}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const loadBookmarks = (fileId, contactId) => (
    async dispatch => {
        try {
            const res = await request.get(`/api/Bookmarks?fileId=${fileId}&contactId=${contactId}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const createBookmark = data => (
    async dispatch => {
        try {
            const res = await request.post('/api/Bookmarks', data);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const deleteBookmark = id => (
    async dispatch => {
        try {
            const res = await request.delete(`/api/Bookmarks/${id}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const loadComments = fileId => (
    async dispatch => {
        try {
            const res = await request.get(`/api/Comments?fileId=${fileId}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const getCommentsByRow = (fileId, rowId) => (
    async dispatch => {
        try {
            const res = await request.get(`/api/comments/getByRowId?fileId=${fileId}&rowId=${rowId}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);


const createComment = data => (
    async dispatch => {
        try {
            const res = await request.post('/api/Comments', data);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const deleteComment = id => (
    async dispatch => {
        try {
            const res = await request.delete(`/api/Comments/${id}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const updateSegment = data => (
    async dispatch => {
        try {
            const res = await request.post(`/api/parser/update`, data);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const finalizeFile = ({fileId, taskId}) => (
    async dispatch => {
        try {
            const res = await request.get(`/api/TranslationValidationTasks/finalizeFile?fileId=${fileId}&taskId=${taskId}`);
            return res.data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const downloadPdfReference = ({fileId, taskId}) => (
  async () => {
      try {
          const res = await request({
              method: 'GET',
              url: `/api/parser/pdf?fileId=${fileId}&taskId=${taskId}`,
              data: {},
              headers: {
                  "Content-type": "application/pdf"
              },
              responseType: 'arraybuffer'
          });
          return res.data;
      } catch (e) { throw e.response && e.response.data }
  }
);

export {
    importFile,
    getAllFilesForTask,
    getFileDetails,
    loadBookmarks,
    createBookmark,
    deleteBookmark,
    loadComments,
    getCommentsByRow,
    createComment,
    deleteComment,
    updateSegment,
    finalizeFile,
    downloadPdfReference
}