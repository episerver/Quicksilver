import * as Actions from './actions';
import { ApplicationState } from './state';
import { InitialState } from './index';

export default function applicationReducer(
  state: ApplicationState = InitialState.application,
  action: Actions.ApplicationActionType
): ApplicationState {
  switch (action.type) {
    case 'SET_MESSAGE':
      return {
        ...state,
        message: action.message
      };
    
    case 'SET_DATA': 
    return {
      ...state,
      data: action.payload
    }
    
    default:
      return state;
  }
}
