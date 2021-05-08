import './css/GalamStyle.css';
import React, { Component } from 'react'
import MainComponent from './components/MainComponent'
import ModMessages from './components/ModMessages';
import { MsgContext } from './consts/MainConst';

export default class App extends Component {
  constructor(props) {
    super(props);
  };

  ModMessagesCall = (msg, isSuccessShow, delMilSec = 3000) => {
    this.ModMessagesCallCh(msg, isSuccessShow, delMilSec);
  }

  ModMessagesCallCh = () => { }

  ReassignModMessagesCallCh = (func) => {
    this.ModMessagesCallCh = func;
  }

  render() {
    return (
      <div>
        <MsgContext.Provider value={this.ModMessagesCall}>
          <MainComponent />
        </MsgContext.Provider>
        <ModMessages ReassignModMessagesCallCh={this.ReassignModMessagesCallCh} />
      </div>
    )
  }
}
