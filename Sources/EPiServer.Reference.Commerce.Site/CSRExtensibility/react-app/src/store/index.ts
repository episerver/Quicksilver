import { combineReducers, createStore } from "redux";

import applicationReducer from './reducer';
import { ApplicationState } from './state';

export interface StoreState {
  application: ApplicationState;
}

export const InitialState: StoreState = {
  application: {
    message: '',
    data: null,
  }
};

export const rootReducer = combineReducers({
  application: applicationReducer
});

const store = createStore(rootReducer, InitialState);

export default store;