import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall, dateRender } from '../../consts/MainConst';
import VsbTable from '../VsbTable';

export default class Approvals extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.data = [];
    this.columns = [
      { name: "Creation Date", data: "CreationDate", render: dateRender },
      { name: "Year", data: "Year" },
      { name: "Quarter", data: "Quarter" },
      { name: "Sale manager", data: "PersonFullName" },
      { name: "Region", data: "Market" },
      { name: "Country", data: "CountryName" },
      { name: "Customer number", data: "CustomerNumber" },
      { name: "Customer name", data: "CustomerName" },
      { name: "End customer", data: "ShipToName" },
      { name: "Item", data: "ItemNumber" },
      { name: "Item description", data: "ItemDescription" },
      { name: "Product family", data: "ProductFamilyName" },
      { name: "Qty", data: "Qty" },
      { name: "", data: "", sortable: false, render: this.iconsRender },
    ];
    this.state = {};
  }

  componentDidMount() { this.getApprovals(); }

  getApprovals = () => {
    fetchCall('GET', `${apiUrl}/Approvals/`, '', this.getApprovalsSuccess, this.getApprovalsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getApprovalsSuccess = (data) => {
    this.data = data.Approvals;
    this.props.ChangeMenuNumOfApprovals({ NumOfApprovals: this.data.length });
    this.setState({});
  }

  getApprovalsError = (error) => { this.MessageShow(error.Message, 0); }

  iconsRender = (row, data, index) => {
    return (
      this.props.logedInUser.Permissions.includes(3) || this.props.logedInUser.Permissions.includes(4) ?
        [<img data-id={row.Id} src="img/ok.png" data-action="allow" className="inLineImg" onClick={this.approvAction} key={`img ${data.Id} ok`} />,
        <img data-id={row.Id} src="img/delete.png" data-action="notAllow" className="inLineImg" onClick={this.approvAction} key={`img ${data.Id} delete`} />] :
        <img data-id={row.Id} src="img/delete.png" data-action="notAllow" className="inLineImg" onClick={this.approvAction} key={`img ${data.Id} delete`} />
    );
  }

  approvAction = (e) => {
    let api = `${apiUrl}/Approvals/?id=${e.currentTarget.dataset.id}&action=${e.currentTarget.dataset.action}`;
    fetchCall('POST', api, '', this.getApprovalsSuccess, this.getApprovalsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  refresh = () => { this.getApprovals(); }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/Refresh.png" onClick={this.refresh} />
          </div>
          <div></div>
        </div>
        <VsbTable
          sortable={true}
          data={this.data}
          columns={this.columns}
          divClass="heightWithTwoMenu"
        />
      </div>
    )
  }
}
