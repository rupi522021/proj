import React, { Component } from 'react'
import { FormControl, InputLabel, OutlinedInput, TextField } from '@material-ui/core';
import Autocomplete from '@material-ui/lab/Autocomplete';
import ModalWidow from './ModalWidow';
import { MsgContext } from '../../consts/MainConst';

export default class AddShipToMod extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.state = {
      customerNumber: '',
      shipToName: '',
      country: null,
      inputValue: ''
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr',
        display: "inline-flex"
      }
    };
  }

  customerNumberChange = (e) => { this.setState({ customerNumber: e.target.value }) }

  shipToNameChange = (e) => { this.setState({ shipToName: e.target.value }) }

  countryChange = (e, newVal) => { this.setState({ country: newVal }) }

  validateForm = () => {
    let valid = true;
    let customerNumberEmptyMsg = "מספר לקוח ריק";
    let shipToNameEmptyMsg = "שם לקוח קצה ריק";
    let countryEmptyMsg = "לא נבחרה מדינה";
    if (this.state.customerNumber == "") {
      valid = false;
      this.MessageShow(customerNumberEmptyMsg, 0);
    }
    if (this.state.shipToName == "") {
      valid = false;
      this.MessageShow(shipToNameEmptyMsg, 0);
    }
    if (this.state.country == null) {
      valid = false;
      this.MessageShow(countryEmptyMsg, 0);
    }
    return valid;
  }

  onClickSave = () => {
    if (this.validateForm()) this.props.saveF({ CustomerNumber: this.state.customerNumber, ShipToName: this.state.shipToName, CountryId: this.state.country.CountryId });
  }

  render() {
    return (
      <ModalWidow cancelX={this.props.cancelX} header={`לקוח קצה חדש`}>
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="customer-number">מספר לקוח</InputLabel>
          <OutlinedInput id="customer-number" value={this.state.customerNumber} onChange={this.customerNumberChange} label="מספר לקוח" type="number" />
        </FormControl>
        <br />
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="ship-to-name">שם לקוח קצה</InputLabel>
          <OutlinedInput id="ship-to-name" value={this.state.shipToName} onChange={this.shipToNameChange} label="שם לקוח קצה" />
        </FormControl>
        <br />
        <Autocomplete
          id="combo"
          onChange={this.countryChange}
          options={this.props.AllCountries}
          getOptionLabel={(option) => option.CountryName}
          style={this.classes.inputStyle}
          renderInput={(params) => <TextField {...params} label="מדינה" variant="outlined" />}
        />
        <br />
        <div className="cntTxt">
          <button className="blueButton" onClick={this.onClickSave}>שמור</button>
          <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
        </div>
      </ModalWidow>
    )
  }
}