import { Checkbox } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import AddShipToMod from '../Modals/AddShipToMod';
import VsbTable from '../VsbTable';

export default class EndCustomers extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.data = [];
    this.columns = [
      { name: "Customer number", data: "CustomerNumber" },
      { name: "Customer name", data: "CustomerName" },
      { name: "End customer", data: "ShipToName" },
      { name: "Country", data: "CountryName" },
      { name: "Active", data: "Active", render: this.chRender },
    ];
    this.state = {
      modalToShow: "",
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr'
      }
    };
  }

  componentDidMount() { this.getShipTos(); }

  getShipTos = () => { fetchCall('GET', `${apiUrl}/ShipTos/`, '', this.getShipTosSuccess, this.getShipTosError, this.props.logedInUser.Token, this.props.logedInUser.UserName); }

  getShipTosSuccess = (data) => {
    this.data = data.ShipTos;
    this.setState({});
  }

  getShipTosError = (error) => { this.MessageShow(error.Message, 0); }

  checkboxChange = (e) => {
    let tmp = {};
    tmp.Active = e.target.checked;
    tmp.CustomerNumber = e.currentTarget.dataset.custumer;
    tmp.ShipToName = e.currentTarget.dataset.shipto;
    fetchCall('POST', `${apiUrl}/ShipTos/`, tmp, this.updateShipTosSuccess, this.updateShipTosError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  updateShipTosSuccess = (data) => {
    this.data.forEach((item, index) => {
      if (data.ShipTo.CustomerNumber == item.CustomerNumber && data.ShipTo.ShipToName == item.ShipToName) this.data[index].Active = data.ShipTo.Active;
    });
    this.MessageShow(data.Message, 1);
    this.setState({});
  }

  updateShipTosError = (error) => { this.MessageShow(error.Message, 0); }

  tttchRender = (row, data, index) => {
    return (row[data]);
  }

  chRender = (row, data, index) => {
    return (<Checkbox
      style={{ padding: 0 }}
      inputProps={{ 'data-custumer': row.CustomerNumber, 'data-shipto': row.ShipToName }}
      checked={row[data]}
      onChange={this.checkboxChange}
      color="primary"
    />);
  }

  addShipTo = () => {
    fetchCall('GET', `${apiUrl}/countriesGet/`, '', this.addShipToSuccess, this.addShipToError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addShipToSuccess = (data) => {
    this.setState({ modalToShow: "AddShipToMod", allCountries: data.Countries });
  }

  addShipToError = (error) => { this.MessageShow(error.Message, 0); }

  refresh = () => { this.getShipTos(); }

  hideModal = () => { this.setState({ modalToShow: "" }); }

  addShipToSave = (data) => {
    fetchCall('PUT', `${apiUrl}/ShipTos/`, data, this.addShipToSaveSuccess, this.addShipToSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addShipToSaveSuccess = (data) => {
    this.MessageShow(data.Message, 1);
    this.data.push(data.ShipTo);
    this.hideModal();
  }

  addShipToSaveError = (error) => { this.MessageShow(error.Message, 0); }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/plus.png" onClick={this.addShipTo} />
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
        {this.state.modalToShow == "AddShipToMod" ? <AddShipToMod saveF={this.addShipToSave} cancelX={this.hideModal} AllCountries={this.state.allCountries} logedInUser={this.props.logedInUser} /> : ""}
        {/* AddShipToMod */}
      </div>
    )
  }
}
