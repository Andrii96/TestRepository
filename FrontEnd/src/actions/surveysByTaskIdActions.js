import * as types from '../constants/actionTypes'
import request from '../httpConfig'

const getSurveyByTaskIdSuccess = (survey, taskId) => (
    {
        type: types.GET_SURVEY_BY_TASK_SUCCESS,
        survey,
        taskId
    }
);

const sendSurveyByTaskIdSuccess = (survey, taskId) => (
    {
        type: types.SEND_SURVEY_BY_TASK_SUCCESS,
        survey,
        taskId
    }
);

const loadSurveyByTaskId = taskId => (
    async dispatch => {
        try {
            const res = await request.get(`/api/Survey?taskId=${taskId}`);
            const data = res.data;
            dispatch(getSurveyByTaskIdSuccess(data, taskId));
            return data;
        } catch (e) { throw e.response && e.response.data }
    }
);

const sendSurveyByTaskId = data => (
    async dispatch => {
        try {
            const res = await request.post('/api/Survey', data);
            const survey = res.data;
            dispatch(sendSurveyByTaskIdSuccess(survey, data.TaskId));
            return data;
        } catch (e) { throw e.response && e.response.data }
    }
);

export {
    loadSurveyByTaskId,
    sendSurveyByTaskId
}