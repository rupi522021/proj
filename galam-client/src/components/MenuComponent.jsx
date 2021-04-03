import React, { Component } from 'react'
import HomeIcon from '@material-ui/icons/Home';
import { MsgContext, apiUrl, fetchCall } from '../consts/MainConst';

export default class MenuComponent extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.Permissions = this.props.logedInUser.Permissions;
    this.state = {
      ApprovalsCount: 0,
    }
  }
  componentDidUpdate() { this.numOfApprovalsGet(); }

  numOfApprovalsGet = () => {
    if (this.Permissions.includes(3) || this.Permissions.includes(4) || this.props.logedInUser.IsSaleMenager)
      fetchCall('GET', `${apiUrl}/numOfApprovals/`, '', this.numOfApprovalsSuccess, this.numOfApprovalsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  numOfApprovalsSuccess = (data) => { if (data.NumOfApprovals != this.state.ApprovalsCount) this.setState({ ApprovalsCount: data.NumOfApprovals }); }

  numOfApprovalsError = (error) => { this.MessageShow(error.Message, 0); }

  render() {
    return (
      <div className="menu">
        <div className={this.props.curentPage == "HomePage" ? "curentTab" : ""}>
          <a data-page="HomePage" onClick={this.props.onClickMenu}><HomeIcon className="homeIconStyle" /></a>
        </div>
        <div className={this.props.curentPage == "MarketingForecast" ? "curentTab" : ""}>
          <a data-page="MarketingForecast" onClick={this.props.onClickMenu}>תחזית שיווק</a>
        </div>
        {this.Permissions.includes(1) || this.Permissions.includes(2) || this.props.logedInUser.IsSaleMenager ?
          <div className={this.props.curentPage == "UpdateMarketingForecast" ? "curentTab" : ""}>
            <a data-page="UpdateMarketingForecast" onClick={this.props.onClickMenu}>עדכון תחזית שיווק</a>
          </div> : ""
        }
        {this.Permissions.includes(7) ?
          <div className={this.props.curentPage == "StatisticalForecast" ? "curentTab" : ""}>
            <a data-page="StatisticalForecast" onClick={this.props.onClickMenu}>תחזית סטטיסטית</a>
          </div> : ""
        }
        <div className={this.props.curentPage == "InventoryForecast" ? "curentTab" : ""}>
          <a data-page="InventoryForecast" onClick={this.props.onClickMenu}>תחזית מלאי</a>
        </div>
        {this.Permissions.includes(3) || this.Permissions.includes(4) || this.props.logedInUser.IsSaleMenager ?
          <div className={this.props.curentPage == "Approvals" ? "curentTab" : ""}>
            <a data-page="Approvals" onClick={this.props.onClickMenu}>בקשות <span className="circle">{this.state.ApprovalsCount}</span></a>
          </div> : ""
        }
        {this.Permissions.includes(6) ?
          <div className={this.props.curentPage == "EndCustomers" ? "curentTab" : ""}>
            <a data-page="EndCustomers" onClick={this.props.onClickMenu}>לקוחות קצה</a>
          </div> : ""
        }
        <div className={this.props.curentPage == "Transactions" ? "curentTab" : ""}>
          <a data-page="Transactions" onClick={this.props.onClickMenu}>תנועות</a>
        </div>
        {this.props.logedInUser.UserTypes.includes(4) ?
          <div className={this.props.curentPage == "UserManagement" ? "curentTab" : ""}>
            <a data-page="UserManagement" onClick={this.props.onClickMenu}>ניהול משתמשים</a>
          </div> : ""
        }
      </div>
    )
  }
}
