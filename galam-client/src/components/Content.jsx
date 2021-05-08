import React, { Component } from 'react'
import { MsgContext } from '../consts/MainConst';
import MenuComponent from './MenuComponent';
import HomePage from './subContents/HomePage';
import Approvals from './subContents/Approvals';
import MarketingForecast from './subContents/MarketingForecast';
import StatisticalForecast from './subContents/StatisticalForecast';
import InventoryForecast from './subContents/InventoryForecast';
import EndCustomers from './subContents/EndCustomers';
import Transactions from './subContents/Transactions';
import UserManagement from './subContents/UserManagement';
import UpdateMarketingForecast from './subContents/UpdateMarketingForecast';

export default class Content extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };

  constructor(props) {
    super(props);
    this.scrDict = {
      "HomePage": HomePage,
      "MarketingForecast": MarketingForecast,
      "UpdateMarketingForecast": UpdateMarketingForecast,
      "StatisticalForecast": StatisticalForecast,
      "InventoryForecast": InventoryForecast,
      "Approvals": Approvals,
      "EndCustomers": EndCustomers,
      "Transactions": Transactions,
      "UserManagement": UserManagement
    };
    this.state = {
      curentPage: "HomePage",
    }
  };

  MovePage = (e) => { this.setState({ curentPage: e.currentTarget.dataset.page }); }

  ChMenuNumOfApprovals = () => { }

  ChangeMenuNumOfApprovals = (data) => { this.ChMenuNumOfApprovals(data); }

  ReassignChangeMenuNumOfApprovals = (func) => { this.ChMenuNumOfApprovals = func; }

  render() {
    const Page = this.scrDict[this.state.curentPage];
    return (
      <div className="contentWoMenu">
        <MenuComponent logedInUser={this.props.logedInUser} onClickMenu={this.MovePage} curentPage={this.state.curentPage}
          ReassignChangeMenuNumOfApprovals={this.ReassignChangeMenuNumOfApprovals} />
        <Page logedInUser={this.props.logedInUser} ChangeMenuNumOfApprovals={this.ChangeMenuNumOfApprovals} />
      </div>
    )
  }
}