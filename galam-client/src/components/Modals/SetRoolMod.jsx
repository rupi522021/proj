import { Checkbox, FormControlLabel, Tab, Tabs } from '@material-ui/core'
import React, { Component } from 'react'
import ModalWidow from './ModalWidow'

export default class SetRoolMod extends Component {
  constructor(props) {
    super(props);
    let chx = this.chxProps();
    this.state = {
      tabValue: 0,
      saveCheckbox: false,
      chxUserTypes: chx.chxUserTypes,
      chxPermissionTypes: chx.chxPermissionTypes
    }
  }

  chxProps = () => {
    let chxUserTypes = this.props.AllUserTypes.map(item => {
      let typeValue = false;
      this.props.user.UserTypes.forEach(type => { if (type == item.Id) typeValue = true });
      return { id: item.Id, name: item.Name, value: typeValue, permissionTypes: item.PermissionTypes };
    });
    let chxPermissionTypes = this.props.AllPermissionTypes.map(item => {
      let permissionValue = false;
      let permissionDisabled = false;
      this.props.user.Permissions.forEach(permission => { if (permission == item.Id) permissionValue = true });
      chxUserTypes.forEach(userType => {
        if (userType.value) {
          userType.permissionTypes.forEach(permissionType => {
            if (permissionType.Id == item.Id) {
              permissionValue = true;
              permissionDisabled = true;
            }
          })
        }
      })
      return { id: item.Id, name: item.Name, value: permissionValue, disabled: permissionDisabled };
    });
    return { chxUserTypes, chxPermissionTypes };
  }

  tabChange = (e, newValue) => {
    this.setState({ tabValue: newValue })
  }

  userTypeChange = (e) => {
    let PermissionTypesOn = [];
    let newUserTypes = this.state.chxUserTypes.map(item => {
      if (item.id == e.currentTarget.dataset.chxid) {
        let tmp = { ...item };
        tmp.value = e.target.checked;
        item.permissionTypes.forEach(i => { PermissionTypesOn.push(i.Id) });
        return tmp;
      }
      else return { ...item };
    })
    let newPermissionTypes = this.state.chxPermissionTypes.map(item => {
      let permissionValue = item.value;
      let permissionDisabled = false;
      newUserTypes.forEach(userType => {
        if (userType.value) {
          userType.permissionTypes.forEach(permissionType => {
            if (permissionType.Id == item.id) {
              permissionValue = true;
              permissionDisabled = true;
            }
          })
        }
      })
      if (item.disabled && !permissionDisabled) permissionValue = false;
      return { id: item.id, name: item.name, value: permissionValue, disabled: permissionDisabled };
    });
    this.setState({ chxUserTypes: newUserTypes, chxPermissionTypes: newPermissionTypes });
  }

  permissioneChange = (e) => {
    let newPermissionTypes = this.state.chxPermissionTypes.map(item => {
      if (item.id == e.currentTarget.dataset.chxid) {
        let tmp = { ...item };
        tmp.value = e.target.checked;
        return tmp;
      }
      else return { ...item };
    })
    this.setState({ chxPermissionTypes: newPermissionTypes });
  }

  onClickSave = () => {
    let user = { ...this.props.user };
    let tmpUserTypes = [];
    let tmpPermissionTypes = [];
    this.state.chxUserTypes.forEach(item => { if (item.value) tmpUserTypes.push(item.id) })
    this.state.chxPermissionTypes.forEach(item => { if (item.value) tmpPermissionTypes.push(item.id) })
    user.UserTypes = tmpUserTypes;
    user.Permissions = tmpPermissionTypes;
    this.props.saveF(user);
  }

  render() {
    return (
      <div>
        <ModalWidow cancelX={this.props.cancelX} header={`עדכון הרשאות ${this.props.user.UserName}`}>
          <Tabs value={this.state.tabValue} onChange={this.tabChange} indicatorColor="primary" textColor="primary" centered >
            <Tab label="הרשאות תפקיד" />
            <Tab label="הרשאות מיוחדות" />
          </Tabs>
          <div className="roolTabDiv">
            <div role="tabpanel" hidden={this.state.tabValue !== 0} id={`tabpanel-${0}`} >
              {this.state.chxUserTypes.map(item =>
                <div key={item.id} className="rightTxt"><FormControlLabel className="marRightCh"
                  control={
                    <Checkbox key={item.id} checked={item.value} onChange={this.userTypeChange} color="primary" inputProps={{ 'data-chxid': item.id }} />
                  }
                  label={item.name} />
                </div>
              )}
            </div>
            <div role="tabpanel" hidden={this.state.tabValue !== 1} id={`tabpanel-${1}`} >
              {this.state.chxPermissionTypes.map(item =>
                <div key={item.id} className="rightTxt"><FormControlLabel className="marRightCh"
                  control={
                    <Checkbox key={item.id} checked={item.value} onChange={this.permissioneChange} color="primary" inputProps={{ 'data-chxid': item.id }} disabled={item.disabled} />
                  }
                  label={item.name} />
                </div>
              )}
            </div>
          </div>
          <div className="cntTxt">
            <button className="blueButton" onClick={this.onClickSave}>שמור</button>
            <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
          </div>
        </ModalWidow>
      </div>
    )
  }
}