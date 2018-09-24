import initialState from './initialState'
import {GET_CURRENT_USER_SUCCESS} from '../constants/actionTypes'

export default function(state = initialState.currentUser, action) {
    switch (action.type) {
        case GET_CURRENT_USER_SUCCESS:
            return action.user;
        default:
            return state
    }
}
